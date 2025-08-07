using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.UpdateIngredient;

public class UpdateIngredientCommand : IRequest<ApiResponse>
{
    public Guid IngredientId { get; set; }
    public string? Name { get; set; }
    public string? MeasureUnit { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateIngredientRequest
{
    public string? Name { get; set; }
    public string? MeasureUnit { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}