using DimPos.Store.Domain.Entities.Common;
using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Entities;

public class Store : EntityAuditBase<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; }
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
    public Guid BrandId { get; set; }
    
    public virtual ICollection<StoreAccounts> StoreAccounts { get; set; } = new List<StoreAccounts>();
    public virtual ICollection<TaxRates> TaxRates { get; set; } = new List<TaxRates>();
    public virtual ICollection<FinancialShiftConfigs>? FinancialShiftConfigs { get; set; } = new List<FinancialShiftConfigs>();
    public virtual ICollection<StorePaymentMethodConfigs>? StorePaymentMethodConfigs { get; set; } = new List<StorePaymentMethodConfigs>();
}