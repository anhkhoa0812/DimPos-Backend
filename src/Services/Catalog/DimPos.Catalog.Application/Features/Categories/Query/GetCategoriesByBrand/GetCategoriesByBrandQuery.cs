using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Filter.FilterModel;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Query.GetCategoriesByBrand;

public class GetCategoriesByBrandQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Name { get; set; }
    public ECategoryType? Type { get; set; }
}