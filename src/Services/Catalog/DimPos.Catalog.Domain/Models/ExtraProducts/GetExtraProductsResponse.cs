namespace DimPos.Catalog.Domain.Models.ExtraProducts;

public class GetExtraProductsResponse
{
    public Guid Id { get; set; }
    public string Name {get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ProductImageForGetExtraProductsResponse>? ProductImages { get; set; }
}
public class ProductImageForGetExtraProductsResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = String.Empty;
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}