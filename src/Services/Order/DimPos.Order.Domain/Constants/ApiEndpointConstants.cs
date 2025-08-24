namespace DimPos.Order.Domain.Constants;

public static class ApiEndpointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class Orders
    {
        public const string OrdersEndpoint = ApiEndpoint + "/orders";
    }

    public static class StorePurchaseOrders
    {
        public const string StorePurchaseOrdersEndpoint = ApiEndpoint + "/store-purchase-orders";
    }

    public static class Dashboards
    {
        public const string DashboardsEndpoint = ApiEndpoint + "/dashboards";
    }
}