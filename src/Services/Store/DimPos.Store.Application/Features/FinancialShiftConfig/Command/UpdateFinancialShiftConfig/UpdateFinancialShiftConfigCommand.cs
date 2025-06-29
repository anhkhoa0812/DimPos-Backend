using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;

public class UpdateFinancialShiftConfigCommand : IRequest<ApiResponse>
{
    public Guid FinancialShiftConfigId { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    
    public bool IsActive { get; set; }
}

public class UpdateFinancialShiftConfigRequest
{
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    
    public bool IsActive { get; set; }
}