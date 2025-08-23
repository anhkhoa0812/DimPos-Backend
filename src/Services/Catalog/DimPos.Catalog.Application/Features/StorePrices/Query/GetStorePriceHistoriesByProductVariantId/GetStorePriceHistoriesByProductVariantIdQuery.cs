using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Query.GetStorePriceHistoriesByProductVariantId;

public class GetStorePriceHistoriesByProductVariantIdQuery : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}