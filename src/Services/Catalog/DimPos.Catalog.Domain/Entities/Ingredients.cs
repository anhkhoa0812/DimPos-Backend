using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class Ingredients : EntityAuditBase<Guid>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? MeasureUnit { get; set; }
    public string? Type { get; set; }
    public int? Status { get; set; }
    public decimal? CostPerUnit { get; set; }
    
    public Guid? BrandId { get; set; }
    
    public virtual IEnumerable<RecipeItems>? RecipeItems { get; set; } = new List<RecipeItems>();
}