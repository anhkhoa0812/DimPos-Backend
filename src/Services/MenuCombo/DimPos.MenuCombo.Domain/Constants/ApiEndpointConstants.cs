namespace DimPos.MenuCombo.Domain.Constants;

public static class ApiEndpointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class BrandMenus
    {
        public const string BrandMenusEndpoint = ApiEndpoint + "/brand-menus";
    }

    public static class StoreMenus
    {
        public const string StoreMenusEndpoint = ApiEndpoint + "/store-menus";
    }
}