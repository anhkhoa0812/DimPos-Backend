namespace DimPos.Store.Domain.Constants;

public static class ApiEndpointConstant
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class Store
    {
        public const string StoreEndpoint = ApiEndpoint + "/stores";
    }
}