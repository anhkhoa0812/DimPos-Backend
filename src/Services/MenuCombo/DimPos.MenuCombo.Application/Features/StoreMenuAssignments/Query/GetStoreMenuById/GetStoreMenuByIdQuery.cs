using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuById;

public class GetStoreMenuByIdQuery : IRequest<ApiResponse>
{
    public Guid StoreMenuId { get; set; }
}