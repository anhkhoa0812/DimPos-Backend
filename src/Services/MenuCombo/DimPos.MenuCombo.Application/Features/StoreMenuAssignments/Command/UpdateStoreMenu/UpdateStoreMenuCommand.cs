using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.UpdateStoreMenu;

public class UpdateStoreMenuCommand : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public Guid StoreMenuId { get; set; }
    public bool IsActiveAtStore { get; set; }
}
public class UpdateStoreMenuRequest
{
    public bool IsActiveAtStore { get; set; }
}