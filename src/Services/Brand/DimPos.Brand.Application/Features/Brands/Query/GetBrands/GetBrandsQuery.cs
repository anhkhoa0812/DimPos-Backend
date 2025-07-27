using DimPos.Brand.Domain.Models.Common;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Query.GetBrands;

public class GetBrandsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Code {get; set;}
    public string? Name { get; set; }
}