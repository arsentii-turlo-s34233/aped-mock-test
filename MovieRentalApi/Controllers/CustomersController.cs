using Microsoft.AspNetCore.Mvc;
using MovieRentalApi.DTOs;
using MovieRentalApi.Repositories;

namespace MovieRentalApi.Controllers;
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}/rentals")]
    public async Task<IActionResult> GetCustomerRentals(int id)
    { 
        if (! await _repository.CustomerExistsAsync(id))
        {
            return NotFound();
        }
        var rentals = await _repository.GetCustomerRentalsAsync(id);
        return Ok(rentals);
    }

    [HttpPost("{id}/rentals/")]

    public async Task<IActionResult> AddRentalAsync(int id, [FromBody] AddRentalDto dto)
    {
        if (!await _repository.CustomerExistsAsync(id))
        {
            return NotFound();
        }

        foreach (var movie in dto.Movies)
        {
            if (!await _repository.MovieExistsAsync(@movie.Title))
            {
                return NotFound();
            }
        }
        await _repository.AddRentalAsync(id, dto);
        return Created($"api/customers/{id}/rentals", null);
    }
}