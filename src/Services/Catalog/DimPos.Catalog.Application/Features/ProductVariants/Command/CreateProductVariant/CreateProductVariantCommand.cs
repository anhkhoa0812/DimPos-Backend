using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.CreateProductVariant;

public class CreateProductVariantCommand : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Size { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Sku { get; set; }
}

public class CreateProductVariantRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Size { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Sku { get; set; }
}