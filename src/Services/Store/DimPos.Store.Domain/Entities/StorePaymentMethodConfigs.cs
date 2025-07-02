using DimPos.Store.Domain.Entities.Common;

namespace DimPos.Store.Domain.Entities;

public class StorePaymentMethodConfigs : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid SystemPaymentMethodTypeId { get; set; }
    public string? CredentialsConfigAtStore { get; set; }
    public bool IsActiveByStore { get; set; }
    
    public virtual Store Store { get; set; }
}