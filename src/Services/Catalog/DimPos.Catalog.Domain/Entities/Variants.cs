using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class Variants : EntityAuditBase<Guid>
{
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public int? Status { get; set; } = 1;
    
    public Guid? BrandId { get; set; }

    public virtual IEnumerable<VariantOptions>? VariantOptions { get; set; } = new List<VariantOptions>();
}