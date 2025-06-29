using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Command.CreateFinancialShiftConfig;

public class CreateFinancialShiftConfigCommand : IRequest<ApiResponse>
{
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
}