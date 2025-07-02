using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class AppliedTaxes : EntityBase<Guid>
{
    public Guid OrderId { get; set; }
    public Guid TaxRateId { get; set; }
    public string TaxNameSnapshot { get; set; } = string.Empty;
    public decimal TaxRateSnapshot { get; set; }
    
    public virtual Orders Order { get; set; }
}