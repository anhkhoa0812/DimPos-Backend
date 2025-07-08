using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateProductVariants;

public class UpdateProductVariantsCommandHandler : IRequestHandler<UpdateProductVariantsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    public UpdateProductVariantsCommandHandler(
        IUnitOfWork<CatalogContext> unitOfWork,
        ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    public async ValueTask<ApiResponse> Handle(UpdateProductVariantsCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId && x.Product.BrandId == brandId,
            include: x => x.Include(p => p.Product)
        );
        if(productVariant == null)
            throw new BadHttpRequestException("Không tìm thấy biến thể sản phẩm với ID đã cung cấp.");
        productVariant.Name = request.UpdateProductVariants.Name ?? productVariant.Name;
        if (request.UpdateProductVariants.Price != null 
            && request.UpdateProductVariants.Price != productVariant.Price)
        {
            var brandPrice = await _unitOfWork.GetRepository<BasePrice>().SingleOrDefaultAsync(
                predicate: x => x.ProductVariantId == productVariant.Id && x.BrandId == brandId
            );
            var newBrandPriceHistory = new BrandPriceHistory()
            {
                Id = Guid.CreateVersion7(),
                BrandPriceId = brandPrice.Id,
                CurrencyCode = brandPrice.CurrencyCode,
                ProductVariantId = request.ProductVariantId,
                NewPrice = (Decimal)request.UpdateProductVariants.Price,
                OldPrice = brandPrice.Price,
                ChangedAt = TimeUtil.GetCurrentSEATime(),
                ChangedBy = _claimService.GetCurrentUserId,
            };
            await _unitOfWork.GetRepository<BrandPriceHistory>().InsertAsync(newBrandPriceHistory);
            brandPrice.Price = (decimal)request.UpdateProductVariants.Price;
            _unitOfWork.GetRepository<BasePrice>().UpdateAsync(brandPrice);
            productVariant.Price = (decimal)request.UpdateProductVariants.Price;
        } 
        if (request.UpdateProductVariants.IsActive != null)
        {
            if (request.UpdateProductVariants.IsActive == false &&
                productVariant.IsActive != request.UpdateProductVariants.IsActive &&
                productVariant.Product.ProductVariants.Count <= 1)
            {
                throw new BadHttpRequestException("Không thể vô hiệu hóa biến thể sản phẩm");
            }
            productVariant.IsActive = request.UpdateProductVariants.IsActive.Value;
        }
        productVariant.IsActive = request.UpdateProductVariants.IsActive ?? productVariant.IsActive;
        productVariant.Size = request.UpdateProductVariants.Size ?? productVariant.Size;
        productVariant.DisplayOrder = request.UpdateProductVariants.DisplayOrder ?? productVariant.DisplayOrder;
        productVariant.Sku = request.UpdateProductVariants.Sku ?? productVariant.Sku;
        productVariant.Description = request.UpdateProductVariants.Description ?? productVariant.Description;
        _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().UpdateAsync(productVariant);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if(!isSuccess)
            throw new Exception("Cập nhật biến thể sản phẩm không thành công");
        return new ApiResponse
        {
            Status = 200,
            Message = "Cập nhật biến thể sản phẩm thành công",
            Data = null
        };
    }
}