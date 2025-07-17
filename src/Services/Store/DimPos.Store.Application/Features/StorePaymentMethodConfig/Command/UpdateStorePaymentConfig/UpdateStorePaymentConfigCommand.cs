using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.UpdateStorePaymentConfig;

public class UpdateStorePaymentConfigCommand : IRequest<ApiResponse>
{
    public Guid StorePaymentMethodConfigId { get; set; }
    public bool IsActiveByStore { get; set; }
}

public class UpdateStorePaymentConfigRequest
{
    public bool IsActiveByStore { get; set; }
}