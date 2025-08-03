using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.UpdateCompleteOrder;

public class UpdateCompleteOrderCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
}