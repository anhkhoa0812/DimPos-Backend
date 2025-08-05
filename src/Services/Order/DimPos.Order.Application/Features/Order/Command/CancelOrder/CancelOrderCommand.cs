using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.CancelOrder;

public class CancelOrderCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}

public class CancelOrderRequest
{
    public string CancellationReason { get; set; } = string.Empty;
}