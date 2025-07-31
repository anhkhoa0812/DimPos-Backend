namespace DimPos.Order.Domain.Models.Settings;

public class GrpcSettings
{
    public string CatalogUrl { get; set; } 
    public string StoreUrl { get; set; }
    public string PromotionUrl { get; set; }
    public string PaymentUrl { get; set; }
    public string InventoryUrl { get; set; }
}