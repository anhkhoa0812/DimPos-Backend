using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateProductVariants;

public class UpdateProductVariantsCommand : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
    public UpdateProductVariantsRequest UpdateProductVariants { get; set; } = new UpdateProductVariantsRequest();
}

public class UpdateProductVariantsRequest
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
    public string? Size { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
}
