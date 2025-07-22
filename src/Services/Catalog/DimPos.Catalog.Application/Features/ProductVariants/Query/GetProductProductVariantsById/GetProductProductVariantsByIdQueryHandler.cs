using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ProductVariants;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductProductVariantsById;

public class GetProductProductVariantsByIdQueryHandler : IRequestHandler<GetProductProductVariantsByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetProductProductVariantsByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    } 
    
    public async ValueTask<ApiResponse> Handle(GetProductProductVariantsByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(GetProductProductVariantsByIdQueryHandler)}: {request.ProductVariantId}");
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId && x.Product.BrandId == brandId
            && x.Product.Type == EProductType.CustomerOrder
            && !x.Product.IsCombo,
            include: x => x.Include(x => x.Product)
        );
        if (productVariant == null)
            throw new BadHttpRequestException("Không tìm thấy biến thể sản phẩm với ID đã cung cấp.");
        var response = ProductVariantMapper.ToGetProductVariantsByIdResponse(productVariant);
        if (response != null)
        {
            if (productVariant.Product.CategoryId != null)
                response.CategoryId = productVariant.Product.CategoryId.Value;
        }
        _logger.Information($"END: {nameof(GetProductProductVariantsByIdQueryHandler)}: {request.ProductVariantId}");

        return new ApiResponse
        {
            Data = response,
            Status = (int) HttpStatusCode.OK,
            Message = "Lấy dữ liệu thành công"
        };
    }
}