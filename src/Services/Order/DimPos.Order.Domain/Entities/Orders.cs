using DimPos.Order.Domain.Entities.Common;
using DimPos.Order.Domain.Enums;

namespace DimPos.Order.Domain.Entities;

public class Orders : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid BrandId { get; set; }
    public Guid? FinancialShiftId { get; set; }
    public EOrderType Type { get; set; }
    public EOrderStatus Status { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public string? CustomerPhoneSnapshot { get; set; }
    public decimal SubTotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; } // This is the amount that has been paid by the customer
    public decimal CashRoundingAmount { get; set; }
    public DateTime? PickupTime { get; set; }
    public string? Note { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancelledByAccountId { get; set; }
    public string SystemPaymentMethodNameSnapshot { get; set; } = string.Empty;
    public Guid SystemPaymentMethodId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedByAccountId { get; set; }
    public int? TableNumberDineIn { get; set; }
    public bool IsNeedToUpdateInventory { get; set; }
    
    public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
    public virtual ICollection<AppliedOrderPromotions> AppliedOrderPromotions { get; set; } = new List<AppliedOrderPromotions>();
    public virtual AppliedTaxes? AppliedTax { get; set; }
}