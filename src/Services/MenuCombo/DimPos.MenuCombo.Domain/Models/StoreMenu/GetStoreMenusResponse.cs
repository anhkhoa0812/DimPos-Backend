namespace DimPos.MenuCombo.Domain.Models.StoreMenu;

public class GetStoreMenusResponse
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public BrandMenuForGetStoreMenuById BrandMenu { get; set; } = null!;
}