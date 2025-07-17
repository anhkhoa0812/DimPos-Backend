using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommand : IRequest<ApiResponse>
{
    public string Code { get; set; } 
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Note { get; set; }
    public string? Sku { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid>? ModifierGroupIds { get; set; }
    public List<CreateProductVariant>? ProductVariants { get; set; }
    
    public List<CreateProductImages>? ProductImages { get; set; }
}

public class CreateProductVariant
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal BrandPrice { get; set; }
    public string? Size { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Sku { get; set; }
}

public class CreateProductImages
{
    public IFormFile Image { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}

public class CreateProductRequest
{
    public string Code { get; set; } 
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Note { get; set; }
    public string? Sku { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid>? ModifierGroupIds { get; set; }
    public List<CreateProductVariant>? ProductVariants { get; set; }
    
    public List<CreateProductImages>? ProductImages { get; set; }
}