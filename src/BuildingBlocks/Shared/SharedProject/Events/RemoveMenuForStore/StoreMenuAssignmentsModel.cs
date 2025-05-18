namespace SharedProject.Events.RemoveMenuForStore;

public class StoreMenuAssignmentsModel
{
    public Guid Id { get; set; }
    public bool IsActiveAtStore { get; set; }
    public DateTime? EffectiveAt { get; set; }
    public DateTime? EffectiveEnd { get; set; }
    public Guid StoreId { get; set; }
    public Guid BrandMenuId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public List<StoreMenuItemAvailabilityModel>? StoreMenuItemAvailability { get; set; } = new();
}
