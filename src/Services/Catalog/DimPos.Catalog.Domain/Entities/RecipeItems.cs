using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class RecipeItems : EntityAuditBase<Guid>
{
    public Guid ProductVariantId { get; set; }
    public bool IsActive { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureSnapshot { get; set; } = string.Empty;
    public Guid? CreatedByAccountId { get; set; }
    
    public virtual ProductVariants ProductVariant { get; set; } = null!;
    public virtual Ingredients Ingredient { get; set; } = null!;
    
}