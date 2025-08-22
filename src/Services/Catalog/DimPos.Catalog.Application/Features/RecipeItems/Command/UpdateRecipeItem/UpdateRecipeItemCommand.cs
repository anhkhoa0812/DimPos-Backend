using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.UpdateRecipeItem;

public class UpdateRecipeItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid RecipeItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class UpdateRecipeItemRequest
{
    public decimal Quantity { get; set; }
}
