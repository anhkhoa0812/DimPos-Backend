using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.RemoveProductComboItem;

public class RemoveProductComboItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid ProductComboItemId { get; set; }
}