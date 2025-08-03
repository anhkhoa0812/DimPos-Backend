using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.ConfirmCashOrder;

public class ConfirmCashOrderCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
    public decimal AmountPaid { get; set; }
}

public class ConfirmCashOrderRequest
{
    public decimal AmountPaid { get; set; }
}