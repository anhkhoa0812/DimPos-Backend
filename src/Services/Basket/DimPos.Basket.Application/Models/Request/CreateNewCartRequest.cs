namespace DimPos.Basket.Application.Models.Request;

public class CreateNewCartRequest
{
    public Guid BrandId { get; set; }
    public decimal? TaxRate { get; set; }
}