using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.CreateModifierOption;

public class CreateModifierOptionCommand : IRequest<ApiResponse>
{
    public Guid ModifierGroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public decimal? PriceDelta { get; set; }
}

public class CreateModifierOptionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public decimal? PriceDelta { get; set; }
}