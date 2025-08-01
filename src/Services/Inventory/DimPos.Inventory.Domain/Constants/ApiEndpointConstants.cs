namespace DimPos.Inventory.Domain.Constants;

public static class ApiEndpointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class InventoryStock
    {
        public const string InventoryStockEndpoint = ApiEndpoint + "inventory-stock";
    }
    
}