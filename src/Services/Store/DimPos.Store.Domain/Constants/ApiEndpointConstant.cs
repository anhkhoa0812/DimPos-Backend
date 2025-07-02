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
    public static class FinancialShiftConfig
    {
        public const string FinancialShiftConfigEndpoint = ApiEndpoint + "/financial-shift-configs";
    }
    public static class FinancialShift
    {
        public const string FinancialShiftEndpoint = ApiEndpoint + "/financial-shifts";
    }

    public static class StorePaymentMethodConfig
    {
        public const string StorePaymentMethodConfigEndpoint = ApiEndpoint + "/store-payment-method-configs";
    }
}