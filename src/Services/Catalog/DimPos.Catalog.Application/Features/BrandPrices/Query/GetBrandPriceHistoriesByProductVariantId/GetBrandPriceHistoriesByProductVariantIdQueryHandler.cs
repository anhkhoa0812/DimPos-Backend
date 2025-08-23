using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.BrandPrices;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.BrandPrices.Query.GetBrandPriceHistoriesByProductVariantId;

public class GetBrandPriceHistoriesByProductVariantIdQueryHandler : IRequestHandler<GetBrandPriceHistoriesByProductVariantIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetBrandPriceHistoriesByProductVariantIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandPriceHistoriesByProductVariantIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var brandPriceHistories = await _unitOfWork.GetRepository<BrandPriceHistory>().GetPagingListAsync(
            selector: x => new GetBrandPriceHistoriesByProductVariantIdResponse()
            {
                Id = x.Id,
                ProductVariantId = x.ProductVariantId,
                OldPrice = x.OldPrice,
                NewPrice = x.NewPrice,
                ChangedAt = x.ChangedAt,
                ChangedBy = x.ChangedBy,
                CurrencyCode = x.CurrencyCode
            },
            predicate: x => x.ProductVariantId == request.ProductVariantId && x.BrandPrice!.BrandId == brandId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "ChangedAt",
            isAsc: request.IsAsc
        );

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy lịch sử giá sản phẩm thành công",
            Data = brandPriceHistories
        };
    }
}