using DimPos.Store.Domain.Entities.Common;

namespace DimPos.Store.Domain.Entities;

public class TaxRates : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsActive { get; set; }
    public Store Store { get; set; } = null!;
}