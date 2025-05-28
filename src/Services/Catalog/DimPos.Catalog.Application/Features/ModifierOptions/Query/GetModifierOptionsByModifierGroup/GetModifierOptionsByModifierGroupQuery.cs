using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsByModifierGroup;

public class GetModifierOptionsByModifierGroupQuery : IRequest<ApiResponse>
{
    public Guid ModifierGroupsId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}