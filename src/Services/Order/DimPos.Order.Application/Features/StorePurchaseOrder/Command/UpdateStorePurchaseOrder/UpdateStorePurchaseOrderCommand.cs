using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.UpdateStorePurchaseOrder;

public class UpdateStorePurchaseOrderCommand : IRequest<ApiResponse>
{
    public Guid StorePurchaseOrderId { get; set; }
    public string? CancellationRequestReasonByStore { get; set; }
    public string? CancellationReasonByBrand { get; set; }
    public string? NoteFromBrand { get; set; }
    public EStorePurchaseOrderStatus Status { get; set; }
    public List<UpdateStorePurchaseOrderItemRequest>? StorePurchaseOrderItemRequests { get; set; }
}
public class UpdateStorePurchaseOrderItemRequest
{
    public Guid Id { get; set; }
    public decimal ApprovedQuantityByBrand { get; set; }
}

public class UpdateStorePurchaseOrderRequest
{
    public string? CancellationRequestReasonByStore { get; set; }
    public string? CancellationReasonByBrand { get; set; }
    public string? NoteFromBrand { get; set; }
    public EStorePurchaseOrderStatus Status { get; set; }
    public List<UpdateStorePurchaseOrderItemRequest>? StorePurchaseOrderItemRequests { get; set; }
}