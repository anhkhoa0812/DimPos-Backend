using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.CreateOrder;

public class CreateOrderCommand : IRequest<ApiResponse>
{
    public Guid BrandId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? PickupTime { get; set; }
    public string? Note { get; set; }
    public EOrderType Type { get; set; }
    public int? TableNumberDineIn { get; set; }
    public Guid StorePaymentMethodConfigId { get; set; }
    public List<CreateOrderItemRequest> OrderItems { get; set; }
    public List<Guid>? PromotionRuleIds { get; set; }
}

public class CreateOrderItemRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public List<Guid>? ModifierOptionIds { get; set; }
}