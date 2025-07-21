using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Command.UpdateStorePrices;

public class UpdateStorePricesCommand : IRequest<ApiResponse>
{
    public Guid StorePriceId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? OverridePrice { get; set; }
}

public class UpdateStorePricesRequest
{
    public string? CurrencyCode { get; set; }
    public decimal? OverridePrice { get; set; }
}