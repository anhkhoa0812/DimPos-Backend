using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class RecipeItems : EntityAuditBase<Guid>
{
    [Required]
    [StringLength(maximumLength: 50)]
    public string? MeasureUnit { get; set; }
    
    [Required]
    public decimal? Quantity { get; set; }
    
    [Required]
    public Guid? RecipeId { get; set; }
    
    [ForeignKey(nameof(RecipeId))]
    public virtual Recipes? Recipe { get; set; }
    
    [Required]
    public Guid? IngredientId { get; set; }
    
    [ForeignKey(nameof(IngredientId))]
    public virtual Ingredients? Ingredient { get; set; }
    
}