using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuByStoreId;

public class GetStoreMenuByStoreIdQuery : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}