using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Query.GetProductVariantsByMenu;

public class GetProductVariantsByMenuQuery : IRequest<ApiResponse>
{
    public Guid BrandMenuId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}