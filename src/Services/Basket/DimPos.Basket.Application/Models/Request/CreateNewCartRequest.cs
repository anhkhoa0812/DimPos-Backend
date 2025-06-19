namespace DimPos.Basket.Application.Models.Request;

public class CreateNewCartRequest
{
    public Guid StoreId { get; set; }
    public Guid BrandId { get; set; }
}