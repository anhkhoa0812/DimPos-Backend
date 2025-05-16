using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommand : IRequest<ApiResponse>
{
    public string Code { get; set; } 
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public decimal? BrandPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? PriceCOGS { get; set; }
    public string Description { get; set; }
    public bool IsAvailable { get; set; }
    public int? DisplayOrder { get; set; }
    public int SaleType { get; set; }
    public string? Note { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? ModifierGroupIds { get; set; }
    public List<CreateProductVariant>? ProductVariants { get; set; }
    
    public List<CreateProductImages>? ProductImages { get; set; }
}

public record CreateProductVariant
{
    public string Code { get; set; }
    public string? AlterativeCode { get; set; }
    public string? Name { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal BrandPrice { get; set; }
    public decimal? PriceCOGS { get; set; }
    public int? DisplayOrder { get; set; }
}

public record CreateProductImages
{
    public IFormFile Image { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}
