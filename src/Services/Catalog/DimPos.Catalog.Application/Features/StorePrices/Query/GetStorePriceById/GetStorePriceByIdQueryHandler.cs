using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.StorePrices;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Query.GetStorePriceById;

public class GetStorePriceByIdQueryHandler : IRequestHandler<GetStorePriceByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStorePriceByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePriceByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");

        var storePrice = await _unitOfWork.GetRepository<StorePrice>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StorePriceId
        );
        if (storePrice == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giá của cửa hàng");
        }
        
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == storePrice.ProductVariantId
                                && x.Product.BrandId == brandId
            );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không có quyền truy cập giá của sản phẩm này");
        }
        
        var response = new GetStorePriceByIdResponse()
        {
            Id = storePrice.Id,
            CurrencyCode = storePrice.CurrencyCode,
            OverridePrice = storePrice.OverridePrice,
            ProductVariant = new ProductVariantByGetStorePriceByIdResponse()
            {
                Id = productVariant.Id,
                Name = productVariant.Name,
                Description = productVariant.Description,
                Size = productVariant.Size,
                IsActive = productVariant.IsActive,
                Price = productVariant.Price,
                Sku = productVariant.Sku,
                Code = productVariant.Code
            }
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy giá của cửa hàng thành công",
            Data = response
        };
    }
}