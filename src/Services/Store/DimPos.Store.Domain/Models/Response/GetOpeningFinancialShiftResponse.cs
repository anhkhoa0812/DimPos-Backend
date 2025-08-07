using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Models.Response;

public class GetOpeningFinancialShiftResponse
{
    public Guid Id { get; set; }
    public DateTime OpeningTimestamp { get; set; }
    public Guid OpenedByAccountId { get; set; } 
    public decimal OpeningCashExpected { get; set; }
    public decimal OpeningCashActual { get; set; }
    public string? OpeningDifferenceReason { get; set; }
    public DateTime? ClosingTimestamp { get; set; }
    public Guid? ClosedByAccountId { get; set; }
    public decimal? TotalGrossSalesInShift { get; set; }
    public decimal? TotalNetSalesInShift { get; set; }
    public decimal? TotalTaxInShift { get; set; }
    public decimal? TotalDiscountInShift { get; set; }
    public decimal? TotalCashRoundingInShift { get; set; }
    public EFinancialShiftStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}