using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Query.GetRecipeItemByProductVariant;

public class GetRecipeItemByProductVariantQuery : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}
