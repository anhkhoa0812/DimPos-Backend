using DimPos.Payment.Domain.Models.Common;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethodById;

public class GetSystemPaymentMethodByIdQuery : IRequest<ApiResponse>
{
    public Guid SystemPaymentMethodId { get; set; }
}