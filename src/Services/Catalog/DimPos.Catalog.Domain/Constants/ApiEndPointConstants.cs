namespace DimPos.Catalog.Domain.Constants;

public static class ApiEndPointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class Products
    {
        public const string ProductsEndpoint = ApiEndpoint + "/products";
    }
    public static class ModifierGroups
    {
        public const string ModifierGroupsEndpoint = ApiEndpoint + "/modifier-groups";
    }
    public static class Categories
    {
        public const string CategoriesEndpoint = ApiEndpoint + "/categories";
    }

    public static class ProductVariants
    {
        public const string ProductVariantsEndpoint = ApiEndpoint + "/product-variants";
    }
    public static class ModifierOptions
    {
        public const string ModifierOptionsEndpoint = ApiEndpoint + "/modifier-options";
    }

    public static class Ingredients
    {
        public const string IngredientsEndpoint = ApiEndpoint + "/ingredients";
    }
}