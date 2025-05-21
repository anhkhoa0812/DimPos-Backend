namespace DimPos.MenuCombo.Domain.Models.StoreMenu;

public class StoreMenuResponse
{
    public List<CategoriesResponse>? Categories { get; set; }
    public List<ProductsResponse>? Products { get; set; }
}
public record CategoriesResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<ChildCategoriesResponse>? ChildCategories { get; set; }
}
public record ChildCategoriesResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public record ProductsResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public List<ProductVariantResponse>? ProductVariants { get; set; }
}

public record ProductVariantResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountPrice { get; set; }
    public decimal Price { get; set; }
    public decimal PriceCOGS { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public bool IsMenuDisplay { get; set; }
    public int DisplayOrder { get; set; }
}