using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;

public class UpdatePaymentMethodCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
    public Guid OldStorePaymentMethodConfigId { get; set; }
    public Guid NewStorePaymentMethodConfigId { get; set; }
}

public class UpdatePaymentMethodRequest
{
    public Guid OldStorePaymentMethodConfigId { get; set; }

    public Guid NewStorePaymentMethodConfigId { get; set; }
}