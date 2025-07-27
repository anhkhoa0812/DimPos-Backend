using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.UpdateProductComboItem;

public class UpdateProductComboItemCommand : IRequest<ApiResponse>
{
    public Guid ProductComboItemId { get; set; }
    public int? Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}
public class UpdateProductComboItemRequest
{
    public int? Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}