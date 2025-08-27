using DimPos.Payment.Domain.Models.Common;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Command.UpdateSystemPaymentMethod;

public class UpdateSystemPaymentMethodCommand : IRequest<ApiResponse>
{
    public Guid SystemPaymentMethodId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsGloballyActive { get; set; }
}
public class UpdateSystemPaymentMethodRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsGloballyActive { get; set; }
}