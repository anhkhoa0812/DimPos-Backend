namespace DimPos.Promotion.Domain.Constants;

public static class ApiEndpointConstants
{
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public static class PromotionRules
    {
        public const string PromotionRulesEndpoint = ApiEndpoint + "/promotion-rules";
    }
    public static class Campaigns
    {
        public const string CampaignsEndpoint = ApiEndpoint + "/campaigns";
    }
    
}