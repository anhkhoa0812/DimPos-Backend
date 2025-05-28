using DimPos.Catalog.Domain.Models.Common;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetProductsById;

public class GetProductsByIdQuery : IRequest<ApiResponse>
{
    public Guid ProductId { get; set; }
}