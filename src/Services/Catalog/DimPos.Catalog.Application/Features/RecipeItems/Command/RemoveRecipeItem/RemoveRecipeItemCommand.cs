using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.RemoveRecipeItem;

public class RemoveRecipeItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid RecipeItemId { get; set; }
}