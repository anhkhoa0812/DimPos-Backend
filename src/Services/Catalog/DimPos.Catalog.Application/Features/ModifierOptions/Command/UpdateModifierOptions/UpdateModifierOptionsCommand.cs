using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.UpdateModifierOptions;

public class UpdateModifierOptionsCommand : IRequest<ApiResponse>
{
    public Guid Id { get; set; }
    public UpdateModifierOptionsRequest UpdateModifierOptions { get; set; } = new();
}

public class UpdateModifierOptionsRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public decimal? PriceDelta { get; set; }
}