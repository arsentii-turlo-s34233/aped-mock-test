using Dapper;
using Microsoft.Data.SqlClient;
using MovieRentalApi.DTOs;

namespace MovieRentalApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        using var conn = new SqlConnection( _connectionString);
        var sql = "SELECT COUNT(1) FROM Customer WHERE customer_id = @Id";
        var result = await conn.ExecuteScalarAsync<int>(sql, new {Id = customerId});
        return result > 0;
    }

    public async Task<CustomerRentalsDto?> GetCustomerRentalsAsync(int customerId)
    {
        using var conn = new SqlConnection(_connectionString);
        var sql = "SELECT c.first_name, c.last_name, r.rental_id, r.rental_date, r.return_date, s.name AS status, m.title, ri.price_at_rental " +
                  "FROM Customer c " +
                  "JOIN Rental r ON c.customer_id = r.customer_id " +
                  "JOIN Status s ON r.status_id = s.status_id " +
                  "JOIN Rental_Item ri ON r.rental_id = ri.rental_id " +
                  "JOIN Movie m ON ri.movie_id = m.movie_id " +
                  "WHERE c.customer_id = @Id;";
        var rows = await conn.QueryAsync(sql, new { Id = customerId });
        var rowList  = rows.ToList();
        if (rowList.Count == 0) return null;
        var firstRow = rowList.First();
        var result = new CustomerRentalsDto
        {
            FirstName = firstRow.first_name,
            LastName = firstRow.last_name,
            Rentals = rowList
                .GroupBy(r => (int)r.rental_id)
                .Select(g =>
                {
                    var first = g.First();
                    return new RentalDto()
                    {
                        Id = g.Key,
                        RentalDate = first.rental_date,
                        ReturnDate = first.return_date,
                        Status = first.status,
                        Movies = g.Select(x => new RentalMovieDto()
                        {
                            Title = x.title,
                            PriceAtRental = x.price_at_rental
                        }).ToList()
                    };
                }).ToList()
        };
        return result;
    }

    public async Task<bool> MovieExistsAsync(string title)
    {
        using var conn = new SqlConnection( _connectionString);
        var sql = "SELECT COUNT(1) FROM Movie WHERE title = @Title";
        var result = await conn.ExecuteScalarAsync<int>(sql, new {Title = title});
        return result > 0;
    }

    public async Task AddRentalAsync(int customerId, AddRentalDto dto)
    {   using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();
        try
        {
            var sql = "INSERT INTO Rental (rental_date, return_date, customer_id, status_id) " +
                      "VALUES (@RentalDate, NULL, @CustomerId, 1); "+
                      "SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var rentalId = await conn.ExecuteScalarAsync<int>(sql, new {RentalDate = dto.RentalDate, CustomerId = customerId}, transaction);
            var sqlMovie = "SELECT movie_id FROM Movie WHERE title = @Title";
            var sqlInsert = "INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental) "+
                                    " VALUES (@RentalId, @MovieId, @Price)";
            foreach (var movie in dto.Movies)
            {
                var movieId = await conn.ExecuteScalarAsync<int>(sqlMovie, new {Title = movie.Title}, transaction);
                await conn.ExecuteAsync(sqlInsert, new { RentalId = rentalId, MovieId = movieId, Price = movie.RentalPrice}, transaction);
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}