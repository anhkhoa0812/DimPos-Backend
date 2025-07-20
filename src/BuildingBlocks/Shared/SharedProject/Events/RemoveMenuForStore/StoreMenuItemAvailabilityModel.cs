namespace SharedProject.Events.RemoveMenuForStore;

public class StoreMenuItemAvailabilityModel
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public Guid BrandMenuItemId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
