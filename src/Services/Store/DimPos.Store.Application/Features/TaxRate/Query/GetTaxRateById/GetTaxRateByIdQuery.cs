using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Query.GetTaxRateById;

public class GetTaxRateByIdQuery : IRequest<ApiResponse>
{
    public Guid TaxRateId { get; set; }
}