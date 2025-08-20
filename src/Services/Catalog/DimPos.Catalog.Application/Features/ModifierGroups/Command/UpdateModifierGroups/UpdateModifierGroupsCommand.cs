using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroups;

public class UpdateModifierGroupsCommand: IRequest<ApiResponse>
{
    public Guid Id { get; set; }
    public UpdateModifierGroupsRequest UpdateModifierGroupsRequest { get; set; }
}

public class UpdateModifierGroupsRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ESelectedTypeModifier? SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public List<UpdateModifierOptions>? ModifierOptions { get; set; }
}
public class UpdateModifierOptions
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}