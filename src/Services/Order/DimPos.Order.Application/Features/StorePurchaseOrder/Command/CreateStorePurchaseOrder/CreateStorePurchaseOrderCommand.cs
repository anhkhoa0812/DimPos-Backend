using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.CreateStorePurchaseOrder;

public class CreateStorePurchaseOrderCommand : IRequest<ApiResponse>
{
    public string? Note { get; set; }
    public List<CreateStorePurchaseOrderItemRequest> StorePurchaseOrderItems { get; set; } = new List<CreateStorePurchaseOrderItemRequest>();
}

public class CreateStorePurchaseOrderItemRequest
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
}