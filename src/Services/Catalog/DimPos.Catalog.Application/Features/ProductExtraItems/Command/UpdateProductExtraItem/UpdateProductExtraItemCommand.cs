using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductExtraItems.Command.CreateProductExtraItem;

public class UpdateProductExtraItemCommand : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
    public List<Guid> ProductVariantItemIds { get; set; } = new List<Guid>();
}

public class UpdateProductExtraItemRequest
{
    public List<Guid> ProductVariantItemIds { get; set; } = new List<Guid>();
}