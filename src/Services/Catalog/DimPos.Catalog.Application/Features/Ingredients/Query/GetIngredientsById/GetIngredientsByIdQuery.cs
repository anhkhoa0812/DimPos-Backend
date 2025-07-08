using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Query.GetIngredientsById;

public class GetIngredientsByIdQuery : IRequest<ApiResponse>
{
    public Guid IngredientId { get; set; }
}