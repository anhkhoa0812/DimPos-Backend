using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Command.CreateTaxRate;

public class CreateTaxRateCommand : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

public class CreateTaxRateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}