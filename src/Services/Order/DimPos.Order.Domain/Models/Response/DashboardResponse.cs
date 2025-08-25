namespace DimPos.Order.Domain.Models.Response;

public class DashboardResponse
{
    
    public decimal TotalDineInRevenue { get; set; }
    public decimal TotalTakeAwayRevenue { get; set; }
    public decimal TotalSubTotalRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscountRevenue { get; set; }
    
    public int TotalDineInOrders { get; set; }
    public int TotalTakeAwayOrders { get; set; }
    public int TotalOrders { get; set; }
    
    public decimal AverageOrderValue { get; set; }
    public decimal AverageDineInOrderValue { get; set; }
    public decimal AverageTakeAwayOrderValue { get; set; }
    
    public decimal AverageItemsPerOrder { get; set; }
    public decimal AverageDineInItemsPerOrder { get; set; }
    public decimal AverageTakeAwayItemsPerOrder { get; set; }
    
    public decimal TotalCashRevenue { get; set; }
    public decimal TotalQrCodeRevenue { get; set; }
    public decimal TotalQrEdcRevenue { get; set; }
    public decimal TotalCardEdcRevenue { get; set; }
}