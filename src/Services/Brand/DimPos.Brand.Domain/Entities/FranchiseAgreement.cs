using DimPos.Brand.Domain.Entities.Common;
using DimPos.Brand.Domain.Enums;

namespace DimPos.Brand.Domain.Entities;

public class FranchiseAgreement : EntityAuditBase<Guid>
{
    public Guid? StoreId { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? InitialFeeCents { get; set; }
    public decimal? RoyaltyPercentage { get; set; }
    public int? Terms { get; set; }
    public EFranchiseAgreementStatus? Status { get; set; }
    
    public Guid? BrandId { get; set; }
    public virtual Brands? Brand { get; set; }
}