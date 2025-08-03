namespace DimPos.Inventory.Domain.Models.Response;

public class GetInventoryStockByIdResponse
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReOrderLevel { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public IngredientsForGetInventoryStockByIdResponse Ingredient { get; set; } = new IngredientsForGetInventoryStockByIdResponse();
}
public class IngredientsForGetInventoryStockByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Code { get; set; }
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
}