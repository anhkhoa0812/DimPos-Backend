namespace DimPos.Notification.Domain.Constants;

public static class ApiEndpointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class Notification
    {
        public const string NotificationEndpoint = ApiEndpoint + "/notifications";
    }
}