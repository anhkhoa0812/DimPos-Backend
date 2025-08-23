using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.StorePrices;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Query.GetStorePriceHistoriesByProductVariantId;

public class GetStorePriceHistoriesByProductVariantIdQueryHandler : IRequestHandler<GetStorePriceHistoriesByProductVariantIdQuery,ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public GetStorePriceHistoriesByProductVariantIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, 
        IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePriceHistoriesByProductVariantIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }
        var brandIdGrpcResponse = await _storeGrpcService.GetBrandIdByStoreIdAsync(
            new GetBrandIdByStoreIdRequest()
            {
                StoreId = request.StoreId.ToString()
            }
        );
        if (brandIdGrpcResponse.BrandId != brandId.ToString())
        {
            throw new BadHttpRequestException("Không có quyền truy cập giá của cửa hàng này");
        }
        var storePriceHistories = await _unitOfWork.GetRepository<StorePriceHistory>().GetPagingListAsync(
            selector: x => new GetStorePriceHistoriesByProductVariantIdResponse()
            {
                Id = x.Id,
                CurrencyCode = x.CurrencyCode,
                OldPrice = x.OldPrice,
                NewPrice = x.NewPrice,
                ChangedAt = x.ChangedAt,
                ChangedBy = x.ChangedBy,
                ProductVariantId = x.ProductVariantId
            },
            predicate: x => x.StoreId == request.StoreId 
                            && x.ProductVariantId == request.ProductVariantId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "ChangedAt",
            isAsc: request.IsAsc
        );

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy lịch sử giá cửa hàng thành công",
            Data = storePriceHistories
        };

    }
}