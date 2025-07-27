using DimPos.Payment.Domain.Models.Common;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethods;

public class GetSystemPaymentMethodsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Name { get; set; }
}