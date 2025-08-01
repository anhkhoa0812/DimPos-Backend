namespace DimPos.Inventory.Domain.Models.Response;

public class GetInventoryStocksResponse
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal ReOrderLevel { get; set; }
    public DateTime LastCountedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public IngredientsForGetInventoryStocksResponse Ingredient { get; set; } = new IngredientsForGetInventoryStocksResponse();
}
public class IngredientsForGetInventoryStocksResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Code { get; set; }
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
}