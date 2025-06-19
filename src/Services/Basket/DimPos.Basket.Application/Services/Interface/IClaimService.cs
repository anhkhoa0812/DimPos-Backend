namespace DimPos.Basket.Application.Services.Interface;

public interface IClaimService
{
    public Guid GetCurrentUserId { get; }
    public string GetCurrentEmail { get; }
    public string GetCurrentUsername { get; }
    public string GetRole { get; }
    public Guid? GetBrandId { get; }
    public Guid? GetStoreId { get; }
}