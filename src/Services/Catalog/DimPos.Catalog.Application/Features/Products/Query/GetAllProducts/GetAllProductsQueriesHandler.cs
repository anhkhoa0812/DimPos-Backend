using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;

public class GetAllProductsQueriesHandler : IRequestHandler<GetAllProductsQueries, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;

    public GetAllProductsQueriesHandler(IUnitOfWork<CatalogContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async ValueTask<ApiResponse> Handle(GetAllProductsQueries request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.GetRepository<Domain.Entities.Products>().GetListAsync();
        var productResponse = ProductMapper.ToProductResponses(products.ToList());
        return new ApiResponse()
        {
            Status = HttpStatusCode.OK,
            Message = "Thành công",
            Data = productResponse
        };
    }
}