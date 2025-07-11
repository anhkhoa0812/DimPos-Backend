using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrderById;

public class GetStorePurchaseOrderByIdQuery : IRequest<ApiResponse>
{
    public Guid StorePurchaseOrderId { get; set; }
}