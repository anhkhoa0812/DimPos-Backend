namespace DimPos.Catalog.Domain.Constants;

public static class ApiEndPointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class Product
    {
        public const string ProductEndpoint = ApiEndpoint + "/products";
    }
}