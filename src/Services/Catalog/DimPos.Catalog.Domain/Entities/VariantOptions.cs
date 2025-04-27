using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class VariantOptions : EntityBase<Guid>
{

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
    public int? DisplayOrder { get; set; }
    public int? Status { get; set; } = 1;
    public Guid? VariantId { get; set; }
    public virtual Variants? Variant { get; set; }
    
    public virtual IEnumerable<ProductVariants>? ProductVariants { get; set; } = new List<ProductVariants>();
    
}