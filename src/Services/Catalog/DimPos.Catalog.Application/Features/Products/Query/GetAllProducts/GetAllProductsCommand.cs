using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;

public class GetAllProductsCommand : IRequest<ApiResponse>
{
}