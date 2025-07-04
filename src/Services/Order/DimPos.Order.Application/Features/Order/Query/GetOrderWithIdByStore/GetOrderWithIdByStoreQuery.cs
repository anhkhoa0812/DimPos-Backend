using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetOrderWithIdByStore;

public class GetOrderWithIdByStoreQuery : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
}