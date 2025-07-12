using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetOrderWithId;

public class GetOrderWithIdQuery : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
}