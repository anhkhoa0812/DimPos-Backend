using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.CreateRecipeItem;

public class CreateRecipeItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
}

public class CreateRecipeItemRequest
{
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
}