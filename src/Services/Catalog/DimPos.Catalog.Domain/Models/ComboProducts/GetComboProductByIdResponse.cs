namespace DimPos.Catalog.Domain.Models.ComboProducts;

public class GetComboProductByIdResponse
{
    public Guid Id { get; set; }
    public string Name {get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ProductImageForGetComboProductByIdResponse>? ProductImages { get; set; }
    public List<ComboProductItemResponse> ComboProductItems { get; set; } = new List<ComboProductItemResponse>();
}
public class ProductImageForGetComboProductByIdResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = String.Empty;
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class ComboProductItemResponse
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public int? DisplayOrder { get; set; }
    public ProductVariantForComboProductItemResponse ProductVariant { get; set; }
}
public class ProductVariantForComboProductItemResponse
{
    public Guid Id { get; set; }
    public string Name {get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
}