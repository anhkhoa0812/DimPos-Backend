namespace DimPos.Catalog.Domain.Models.InternalProducts;

public class GetInternalProductByIdResponse
{
    public Guid Id { get; set; }
    public string Name {get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ProductImageForGetInternalProductByIdResponse>? ProductImages { get; set; }
    public List<RecipeItemsForGetInternalProductByIdResponse>? RecipeItems { get; set; }
}
public class ProductImageForGetInternalProductByIdResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = String.Empty;
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class RecipeItemsForGetInternalProductByIdResponse
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public IngredientForGetInternalProductByIdResponse Ingredient { get; set; } = new IngredientForGetInternalProductByIdResponse();
}

public class IngredientForGetInternalProductByIdResponse
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