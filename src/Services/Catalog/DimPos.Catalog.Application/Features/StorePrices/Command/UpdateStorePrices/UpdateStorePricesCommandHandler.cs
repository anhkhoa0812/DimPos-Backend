using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;

namespace DimPos.Catalog.Application.Features.StorePrices.Command.UpdateStorePrices;

public class UpdateStorePricesCommandHandler : IRequestHandler<UpdateStorePricesCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateStorePricesCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStorePricesCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin tài khoản người dùng");
        }
        var storePrices = await _unitOfWork.GetRepository<StorePrice>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StorePriceId
        );
        if (storePrices == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giá của cửa hàng");
        }

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>()
            .SingleOrDefaultAsync(
                predicate: x => x.Id == storePrices.ProductVariantId
                                && x.Product.BrandId == brandId
            );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không có quyền cập nhật giá của sản phẩm này");
        }
        storePrices.CurrencyCode = request.CurrencyCode ?? storePrices.CurrencyCode;
        if (request.OverridePrice != null)
        {
            storePrices.StorePriceHistories?.Add(new StorePriceHistory()
            {
                Id = Guid.CreateVersion7(),
                OldPrice = storePrices.OverridePrice,
                NewPrice = request.OverridePrice.Value,
                CurrencyCode = storePrices.CurrencyCode,
                StoreId = storePrices.StoreId,
                ChangedAt = TimeUtil.GetCurrentSEATime(),
                StorePriceId = storePrices.Id,
                ChangedBy = accountId,
                ProductVariantId = storePrices.ProductVariantId
            });
        }
        _unitOfWork.GetRepository<StorePrice>().UpdateAsync(storePrices);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật giá của cửa hàng không thành công, vui lòng thử lại sau");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật giá của cửa hàng thành công",
            Data = storePrices.Id
        };
    }
}