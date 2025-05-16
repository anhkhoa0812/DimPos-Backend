using DimPos.Brand.Domain.Entities.Common;

namespace DimPos.Brand.Domain.Entities;

public class BrandAccounts : EntityAuditBase<Guid>
{
    public Guid AccountId { get; set; }
    public Guid BrandId { get; set; }
    public virtual Brands Brand { get; set; }
}