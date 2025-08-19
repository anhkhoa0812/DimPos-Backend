using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProducts;

public class GetExtraProductsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Name { get; set; }
    public string? Sku { get; set; }
}