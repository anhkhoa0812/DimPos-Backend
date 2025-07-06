namespace DimPos.Catalog.Domain.Models.RecipeItems;

public class GetRecipeItemByProductVariantResponse
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public IngredientWithRecipeItemByProductVariantResponse Ingredient { get; set; }
}
public class IngredientWithRecipeItemByProductVariantResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}