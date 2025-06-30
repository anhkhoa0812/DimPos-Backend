using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigs;

public class GetFinancialShiftConfigsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}