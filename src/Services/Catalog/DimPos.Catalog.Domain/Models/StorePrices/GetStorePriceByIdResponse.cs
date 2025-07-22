namespace DimPos.Catalog.Domain.Models.StorePrices;

public class GetStorePriceByIdResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = String.Empty;
    public decimal OverridePrice { get; set; }
    public ProductVariantByGetStorePriceByIdResponse ProductVariant { get; set; }
}
public class ProductVariantByGetStorePriceByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
}