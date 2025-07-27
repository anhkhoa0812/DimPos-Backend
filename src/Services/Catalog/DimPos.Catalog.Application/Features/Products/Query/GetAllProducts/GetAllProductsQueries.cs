using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;

public class GetAllProductsQueries : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Name { get; set; }
    public bool? IsHasVariants { get; set; }
}