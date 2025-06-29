using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;

public class OpenFinancialShiftCommand : IRequest<ApiResponse>
{
    public decimal OpeningCashActual { get; set; }
    public string? OpeningDifferenceReason { get; set; }
}