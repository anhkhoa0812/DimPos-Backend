using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductProductVariantsById;

public class GetProductProductVariantsByIdQuery:  IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
}