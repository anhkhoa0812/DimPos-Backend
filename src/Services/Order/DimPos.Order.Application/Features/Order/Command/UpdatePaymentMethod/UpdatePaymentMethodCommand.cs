using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;

public class UpdatePaymentMethodCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
    public Guid StorePaymentMethodConfigId { get; set; }
}

public class UpdatePaymentMethodRequest
{
    public Guid StorePaymentMethodConfigId { get; set; }
}