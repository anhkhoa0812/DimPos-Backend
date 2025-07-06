using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.CreateIngredient;

public class CreateIngredientCommand : IRequest<ApiResponse>
{
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MeasureUnit { get; set; } = string.Empty;
    public string? Description { get; set; }
}