using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Models.ProductVariants;

public class GetProductVariantsByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProductId { get; set; }
    public List<RecipeItemsForGetProductVariantsByIdResponse>? RecipeItems { get; set; }
}
public class RecipeItemsForGetProductVariantsByIdResponse
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public IngredientForGetProductVariantsByIdResponse Ingredient { get; set; } = new IngredientForGetProductVariantsByIdResponse();
}
public class IngredientForGetProductVariantsByIdResponse
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