using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductVariantsForMenu;

public class GetProductVariantsForMenuQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public bool? IsCombo { get; set; }
    public bool? IsExtra { get; set; }
}