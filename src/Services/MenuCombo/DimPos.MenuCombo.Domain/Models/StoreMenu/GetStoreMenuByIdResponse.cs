using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Models.StoreMenu;

public class GetStoreMenuByIdResponse
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public BrandMenuForGetStoreMenuById BrandMenu { get; set; } = null!;
    public List<StoreMenuItemForGetStoreMenuById>? StoreMenuItems { get; set; } = new List<StoreMenuItemForGetStoreMenuById>();
}

public class BrandMenuForGetStoreMenuById()
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public EBrandMenuType? Type { get; set; }
    public bool IsActiveByBrand { get; set; }
}
public class StoreMenuItemForGetStoreMenuById
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public ProductVariantResponse ProductVariant { get; set; } = null!;
}