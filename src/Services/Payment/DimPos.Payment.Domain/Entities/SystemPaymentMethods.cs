using DimPos.Payment.Domain.Entities.Common;
using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Entities;

public class SystemPaymentMethods : EntityAuditBase<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public ESystemPaymentMethod Type { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool IsGloballyActive { get; set; }
    public string ConfigurationSchema { get; set; }
    
    public virtual ICollection<PaymentTransactions> PaymentTransactions { get; set; } = new List<PaymentTransactions>();
}