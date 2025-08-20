using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Models.StoreMenu;

public class StoreMenuResponse
{
    public decimal TaxRate { get; set; }
    public Guid BrandId { get; set; }
    public List<CategoriesResponse>? Categories { get; set; }
    public List<ProductsResponse>? Products { get; set; }
    public List<ModifierGroupsResponses>? ModifierGroups { get; set; }
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
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public List<ProductVariantResponse>? ProductVariants { get; set; }
    public List<ComboItemsResponse>? ComboItems { get; set; }
    public List<ProductVariantResponse>? ExtraItemProductVariants { get; set; }
}

public record ProductVariantResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public int DisplayOrder { get; set; }
    public string? Sku { get; set; }
}
public record ModifierGroupsResponses
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ESelectedTypeModifier SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid BrandId { get; set; }
    public List<Guid> ProductVariantIds { get; set; } = new();
    public List<ModifierOptionsResponses>? ModifierOptions { get; set; } = new();
}
public record ModifierOptionsResponses
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid ModifierGroupId { get; set; }
}
public record ComboItemsResponse
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
    public int Quantity { get; set; }
    public ProductVariantResponse ProductVariant { get; set; } = null!;
}