using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuByBrand;

public class GetBrandMenuByBrandQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}