using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Test;

public class GetProductForOrderQuery : IRequest<ApiResponse>
{
    public string StoreId { get; set; }
    public string BrandId { get; set; }
    public List<ProductForOrders> ProductForOrders { get; set; } = new List<ProductForOrders>();
}

public class ProductForOrders
{
    public string Id { get; set; }
    public int Quantity { get; set; }
    public List<ModifierOptionsForOrders> ModifierOptions { get; set; } = new List<ModifierOptionsForOrders>();
}

public class ModifierOptionsForOrders
{
    public string ModifierOptionId { get; set; }
    public string? RelatedComboProductVariantItemId { get; set; }
}