namespace DimPos.Catalog.Domain.Models.ExtraProducts;

public class GetExtraProductByIdResponse
{
    public Guid Id { get; set; }
    public string Name {get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ProductImageForGetExtraProductByIdResponse>? ProductImages { get; set; }
    public List<RecipeItemsForGetExtraProductByIdResponse>? RecipeItems { get; set; } = new List<RecipeItemsForGetExtraProductByIdResponse>();
}

public class RecipeItemsForGetExtraProductByIdResponse
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public IngredientForGetExtraProductByIdResponse Ingredient { get; set; } = new IngredientForGetExtraProductByIdResponse();
}

public class IngredientForGetExtraProductByIdResponse
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

public class ProductImageForGetExtraProductByIdResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = String.Empty;
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}