using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsById;

public class GetModifierOptionsByIdQuery : IRequest<ApiResponse>
{
    public Guid ModifierOptionId { get; set; }
}