namespace MovieRentalApi.DTOs;

public class AddRentalDto
{
    public DateTime RentalDate { get; set; }
    public List<RentalMovieInputDto> Movies { get; set; }
}
public class RentalMovieInputDto
{
    public string Title { get; set; }
    public decimal RentalPrice { get; set; }
}