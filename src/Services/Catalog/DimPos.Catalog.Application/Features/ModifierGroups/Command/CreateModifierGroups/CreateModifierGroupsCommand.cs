using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;

public class CreateModifierGroupsCommand : IRequest<ApiResponse>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public ESelectedTypeModifier SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public int? Status { get; set; }
    public List<CreateModifierOptions>? ModifierOptions { get; set; } = new List<CreateModifierOptions>();
}

public record CreateModifierOptions
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public decimal? PriceDelta { get; set; }
}