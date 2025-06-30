using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigById;

public class GetFinancialShiftConfigByIdQuery : IRequest<ApiResponse>
{
    public Guid FinancialShiftConfigId { get; set; }
}