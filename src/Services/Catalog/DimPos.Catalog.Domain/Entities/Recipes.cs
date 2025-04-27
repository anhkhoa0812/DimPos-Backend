using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class Recipes : EntityAuditBase<Guid>
{
    public string? Version { get; set; }
    
    public bool? IsActive { get; set; } = true;
    
    public Guid? ProductVariantId { get; set; }
    
    public virtual IEnumerable<RecipeItems>? RecipeItems { get; set; } = new List<RecipeItems>();
    
    public virtual ProductVariants? ProductVariant { get; set; }
}