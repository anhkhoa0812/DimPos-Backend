using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.InternalProducts.Query.GetInternalProductById;

public class GetInternalProductByIdQuery : IRequest<ApiResponse>
{
    public Guid ProductVariantId { get; set; }
}