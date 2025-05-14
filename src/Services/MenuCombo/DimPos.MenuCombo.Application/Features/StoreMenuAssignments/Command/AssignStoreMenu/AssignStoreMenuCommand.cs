using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.AssignStoreMenu;

public class AssignStoreMenuCommand: IRequest<ApiResponse>
{
    public Guid BrandMenuId { get; set; }
    public List<AssignStoreMenuRequest> AssignStoreMenuRequests { get; set; }
}
public class AssignStoreMenuRequest
{
    public Guid StoreId { get; set; }
    public DateTime? EffectiveAt { get; set; }
    public DateTime? EffectiveEnd { get; set; }
}