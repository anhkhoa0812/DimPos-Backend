using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ComboProducts.Command.CreateComboProduct;

public class CreateComboProductCommand : IRequest<ApiResponse>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Note { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; } 
    public List<CreateItemProductVariant> ItemProductVariants { get; set; } = new List<CreateItemProductVariant>();
    public List<CreateComboProductImages>? ProductImages { get; set; }
}
public class CreateComboProductImages
{
    public IFormFile Image { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class CreateItemProductVariant
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public int? DisplayOrder { get; set; }
}