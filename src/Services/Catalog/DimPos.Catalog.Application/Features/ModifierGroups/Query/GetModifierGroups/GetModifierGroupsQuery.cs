using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroups;

public class GetModifierGroupsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 30;
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; } = true;
}