using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class Ingredients : EntityAuditBase<Guid>
{
    public Guid BrandId { get; set; }
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    public virtual ICollection<RecipeItems>? RecipeItems { get; set; } = new List<RecipeItems>();
    public virtual ICollection<UnitConversions>? UnitConversions { get; set; } = new List<UnitConversions>();
}