using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.CreateProductComboItem;

public class CreateProductComboItemCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public Guid ProductVariantItemId { get; set; }
    public int Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}

public class CreateProductComboItemRequest
{
    public Guid ProductVariantItemId { get; set; }
    public int Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}