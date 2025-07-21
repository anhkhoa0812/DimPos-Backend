namespace DimPos.Catalog.Domain.Models.StorePrices;

public class GetAllStorePricesByStoreIdResponse
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; }
    public decimal OverridePrice { get; set; }
    public ProductVariantByGetAllStorePricesByStoreIdResponse ProductVariant { get; set; }
}
public class ProductVariantByGetAllStorePricesByStoreIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}