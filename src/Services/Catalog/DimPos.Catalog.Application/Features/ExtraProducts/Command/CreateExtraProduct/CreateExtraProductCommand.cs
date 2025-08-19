using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Command.CreateExtraProduct;

public class CreateExtraProductCommand : IRequest<ApiResponse>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Note { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; } 
    public List<CreateExtraProductImages>? ProductImages { get; set; }
}
public class CreateExtraProductImages
{
    public IFormFile Image { get; set; }
    public bool IsMainImage { get; set; }
    public string? AltText { get; set; }
}