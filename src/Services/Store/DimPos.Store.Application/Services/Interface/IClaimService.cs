namespace DimPos.Store.Application.Services.Interface;

public class IClaimService
{
    public Guid GetCurrentUserId { get; }
    public string GetCurrentEmail { get; }
    public string GetCurrentUsername { get; }
    public string GetRole { get; }
    public Guid? GetBrandId { get; }
    public Guid? GetStoreId { get; }
}