using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroupsById;

public class GetModifierGroupsByIdQuery : IRequest<ApiResponse>
{
    public Guid ModifierGroupId { get; set; }
}