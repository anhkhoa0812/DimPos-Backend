using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class UnitConversions : EntityBase<Guid>
{
    public Guid IngredientId { get; set; }
    public string FromUnit { get; set; } = string.Empty;
    public string ToUnit { get; set; } = string.Empty;
    public decimal Factor { get; set; }
    
    public virtual Ingredients Ingredient { get; set; } = null!;
}