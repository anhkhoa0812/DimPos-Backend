using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetTableNumberDineIn;

public class GetTableNumberDineInQuery : IRequest<ApiResponse>
{
    public Guid FinancialShiftId { get; set; }
}