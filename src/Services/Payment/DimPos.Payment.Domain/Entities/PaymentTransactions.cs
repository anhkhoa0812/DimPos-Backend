using DimPos.Payment.Domain.Entities.Common;
using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Entities;

public class PaymentTransactions : EntityAuditBase<Guid>
{
    public Guid? OrderId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ETransactionType TransactionType { get; set; }
    public string? PspReference { get; set; }
    public EPaymentTransactionStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionTime { get; set; }
    public Guid? ProcessedByAccountId { get; set; }
    public Guid? RelatedPaymentTransactionId { get; set; }
    public Guid? FinancialShiftId { get; set; }
    public Guid SystemPaymentMethodTypeId { get; set; }
    public virtual SystemPaymentMethods SystemPaymentMethodType { get; set; }
}