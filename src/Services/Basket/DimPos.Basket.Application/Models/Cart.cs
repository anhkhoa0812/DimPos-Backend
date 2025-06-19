using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models;

public class Cart
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? PosDeviceId { get; set; }
    public Guid? StaffAccountIdCreating { get; set; }
    public Guid? CustomerIdLinked { get; set; }
    public EServiceMethod ServiceMethod { get; set; }
    public int? TakeNumberDineIn { get; set; }
    public DateTime? PickupTimeRequested { get; set; }
    public ECartStatus Status { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TotalItemDiscountAmount { get; set; }
    public decimal OrderLevelDiscountAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal FinalTotalAmount { get; set; }
    public string? CustomerNotesForOrder { get; set; }
    public string? StaffNotesForOrder { get; set; }
    public Guid? SelectedPaymentMethodConfigId { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantityOfItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
}