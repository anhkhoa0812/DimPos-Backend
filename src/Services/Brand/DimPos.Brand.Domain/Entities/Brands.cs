using DimPos.Brand.Domain.Entities.Common;
using DimPos.Brand.Domain.Enums;

namespace DimPos.Brand.Domain.Entities;

public class Brands : EntityAuditBase<Guid>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public EBrandStatus? Status { get; set; }
    public virtual ICollection<BrandAccounts> BrandAccounts { get; set; } = new List<BrandAccounts>();
    public virtual ICollection<FranchiseAgreement> FranchiseAgreements { get; set; } = new List<FranchiseAgreement>();
}