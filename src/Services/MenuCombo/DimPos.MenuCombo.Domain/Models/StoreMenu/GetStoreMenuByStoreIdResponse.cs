using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Models.StoreMenu;

public class GetStoreMenuByStoreIdResponse
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public BrandMenuForGetStoreMenuByStoreIdResponse BrandMenu { get; set; }
}

public class BrandMenuForGetStoreMenuByStoreIdResponse()
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string? Description { get; set; }
    public EBrandMenuType? Type { get; set; }
    public bool IsActiveByBrand { get; set; } 
}