namespace MovieRentalApi.DTOs;

public class CustomerRentalsDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<RentalDto> Rentals { get; set; }
}
public class RentalDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; }
    public List<RentalMovieDto> Movies { get; set; }
}

public class RentalMovieDto
{
    public string Title { get; set; }
    public decimal PriceAtRental { get; set; }
    
}
