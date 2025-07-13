using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.FinancialShift.Query.GetFinancialShiftById;

public class GetFinancialShiftByIdQuery : IRequest<ApiResponse>
{
    public Guid FinancialShiftId { get; set; }
}