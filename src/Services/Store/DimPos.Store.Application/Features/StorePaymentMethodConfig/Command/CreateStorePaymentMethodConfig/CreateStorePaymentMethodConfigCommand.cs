using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.CreateStorePaymentMethodConfig;

public class CreateStorePaymentMethodConfigCommand : IRequest<ApiResponse>
{
    public Guid SystemPaymentMethodId { get; set; }
    public string? CredentialsConfigAtStore { get; set; }
}