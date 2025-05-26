using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.Product;

public class ProductResponse
{
    public Guid Id { get; set;}
    public string Code { get; set; }
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsHasVariants { get; set; } = false;
    public bool IsHasRecipe { get; set; } = false;
    public EProductStatus Status { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int? DisplayOrder { get; set; }
    public bool IsMenuDisplay { get; set; } = false;
    public EProductSaleType SaleType { get; set; }
    public bool IsMostOrdered { get; set; }
    public string? Note { get; set; }
    public int? NumOfUserVoted { get; set; } 
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public List<ProductVariantsResponse> ProductVariants { get; set; } = new List<ProductVariantsResponse>();
    public List<ProductImagesResponse>? ProductImages { get; set; } = new List<ProductImagesResponse>();
}
public record ProductVariantsResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? AlternativeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceCOGS { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public bool? IsMenuDisplay { get; set; }
    public EProductVariantStatus Status { get; set; }
}
public record ProductImagesResponse {
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } 
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}