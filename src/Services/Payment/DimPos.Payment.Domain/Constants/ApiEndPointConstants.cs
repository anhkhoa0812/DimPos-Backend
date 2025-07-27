namespace DimPos.Payment.Domain.Constants;

public static class ApiEndPointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class SystemPaymentMethods
    {
        public const string SystemPaymentMethodsEndpoint = ApiEndpoint + "/system-payment-methods";
    }
}