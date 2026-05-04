using MovieRentalApi.DTOs;

namespace MovieRentalApi.Repositories;

public interface ICustomerRepository
{
    public Task<bool> CustomerExistsAsync(int customerId);
    public Task<CustomerRentalsDto> GetCustomerRentalsAsync(int customerId); //Get endpoint
    public Task<bool> MovieExistsAsync(string title);
    public Task AddRentalAsync(int customerId, AddRentalDto dto); //Post endpoint
}