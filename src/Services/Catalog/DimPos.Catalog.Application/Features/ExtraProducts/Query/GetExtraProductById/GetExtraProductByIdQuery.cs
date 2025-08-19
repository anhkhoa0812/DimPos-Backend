using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProductById;

public class GetExtraProductByIdQuery : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
}