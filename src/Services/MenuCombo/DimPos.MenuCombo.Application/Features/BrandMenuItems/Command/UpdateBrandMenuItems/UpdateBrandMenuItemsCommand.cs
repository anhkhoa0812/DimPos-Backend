using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Command.UpdateBrandMenuItems;

public class UpdateBrandMenuItemsCommand : IRequest<ApiResponse>
{
    public UpdateBrandMenuItemsRequest UpdateBrandMenuItemsRequest { get; set; } = new();
    public Guid BrandMenuId { get; set; }
}

public record UpdateBrandMenuItemsRequest
{
    public List<Guid>? ProductVariantIds { get; set; }
}