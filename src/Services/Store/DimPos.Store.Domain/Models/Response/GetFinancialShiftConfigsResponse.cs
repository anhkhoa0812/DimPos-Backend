namespace DimPos.Store.Domain.Models.Response;

public class GetFinancialShiftConfigsResponse
{
    public Guid Id { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public Guid CreatedByAccountId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}