using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.UpdateStoreMenuItem;

public class UpdateStoreMenuItemCommand : IRequest<ApiResponse>
{
    public Guid StoreMenuItem { get; set; }
    public List<Guid>? ProductVariantIds { get; set; } = new();
}
public class UpdateStoreMenuItemRequest
{
    public List<Guid>? ProductVariantIds { get; set; } = new();
}