using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Models.Response;

public class GetStoreResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public EStoreStatus Status { get; set; }
    public string? WifiName { get; set; }
    public string? WifiPassword { get; set; }
    public int? Index { get; set; }
    public string? LocalPasscode { get; set; }
    public string? ManagerName { get; set; }
    public decimal StartingStoreCashLending { get; set; }
    public EStoreType Type { get; set; }
    public string? PictureUrl { get; set; }
    public Guid BrandId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public TaxRateForGetStoreResponse? TaxRate { get; set; }
}
public class TaxRateForGetStoreResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}