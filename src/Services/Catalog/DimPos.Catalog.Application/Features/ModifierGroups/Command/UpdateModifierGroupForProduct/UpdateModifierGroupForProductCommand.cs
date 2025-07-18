using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroupForProduct;

public class UpdateModifierGroupForProductCommand : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
    public List<Guid> ModifierGroupIds { get; set; }
}
public class UpdateModifierGroupForProductRequest
{
    public List<Guid> ModifierGroupIds { get; set; } = new();
}