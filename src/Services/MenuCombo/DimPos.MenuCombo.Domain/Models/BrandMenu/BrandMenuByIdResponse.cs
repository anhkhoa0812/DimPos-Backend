using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Models.BrandMenu;

public class BrandMenuByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EBrandMenuType? Type { get; set; }
    public bool IsActiveByBrand { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public List<BrandMenuByIdResponseWithProductVariants> ProductVariants { get; set; } = new List<BrandMenuByIdResponseWithProductVariants>();
    public List<BrandMenuByIdResponseWithStore> Stores { get; set; } = new List<BrandMenuByIdResponseWithStore>();
}
public class BrandMenuByIdResponseWithProductVariants : BrandMenuByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
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
    public EProductVariantStatus Status { get; set; }
    public string? Sku { get; set; }
}

public class BrandMenuByIdResponseWithStore
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public EStoreStatus Status { get; set; }
}