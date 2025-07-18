using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.UpdateRecipeItem;

public class UpdateRecipeItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid RecipeItemId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateRecipeItemRequest
{
    public int Quantity { get; set; }
}
