using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;

public class GetAllProductsQueriesHandler : IRequestHandler<GetAllProductsQueries, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly IClaimService _claimService;

    public GetAllProductsQueriesHandler(IUnitOfWork<CatalogContext> unitOfWork, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(GetAllProductsQueries request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var products = await _unitOfWork.GetRepository<Domain.Entities.Products>().GetListAsync(
            predicate: x => x.BrandId == brandId
        );
        var productResponse = ProductMapper.ToProductResponses(products.ToList());
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.OK,
            Message = "Thành công",
            Data = productResponse
        };
    }
}