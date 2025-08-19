using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Command.UpdateExtraProduct;

public class UpdateExtraProductCommandHandler : IRequestHandler<UpdateExtraProductCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateExtraProductCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateExtraProductCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");
        
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.Id
                            && x.Product.Type == EProductType.CustomerOrder
                            && !x.Product.IsCombo
                            && x.Product.IsExtra
                            && x.Product.BrandId == brandId,
            include: x => x.Include(p => p.Product)
                .ThenInclude(x => x.ProductImages)
                .Include(x => x.RecipeItems)
        );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm extra");
        }
        
        if (request.IsActive != null)
        {
            if (request.IsActive == true &&
                (productVariant.RecipeItems == null || !productVariant.RecipeItems.Any()) )
            {
                throw new BadHttpRequestException("Không thể kích hoạt biến thể sản phẩm extra, vui lòng thêm công thức cho biến thể sản phẩm extra");
            }
            
            productVariant.IsActive = request.IsActive.Value;
        }
        
        if (request.Name != null)
        {
            productVariant.Name = request.Name;
            productVariant.Product.Name = request.Name;
        }
        if (request.Description != null)
        {
            productVariant.Description = request.Description;
            productVariant.Product.Description = request.Description;
        }

        if (request.DisplayOrder != null)
        {
            productVariant.DisplayOrder = request.DisplayOrder;
            productVariant.Product.DisplayOrder = request.DisplayOrder;
        }
        
        if (request.Price != null)
        {
            var basePrice = await _unitOfWork.GetRepository<BasePrice>().SingleOrDefaultAsync(
                predicate: x => x.ProductVariantId == productVariant.Id 
                                && x.BrandId == brandId
            );
            basePrice.Price = request.Price.Value;
            await _unitOfWork.GetRepository<BrandPriceHistory>().InsertAsync(new BrandPriceHistory()
            {
                Id = Guid.CreateVersion7(),
                ProductVariantId = productVariant.Id,
                ChangedAt = TimeUtil.GetCurrentSEATime(),
                ChangedBy = accountId,
                OldPrice = productVariant.Price,
                NewPrice = request.Price.Value,
                CurrencyCode = "VND",
                BrandPriceId = basePrice.Id
            });
            productVariant.Price = request.Price.Value;
            _unitOfWork.GetRepository<BasePrice>().UpdateAsync(basePrice);
        }
        _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().UpdateAsync(productVariant);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật sản phẩm extra không thành công");
        }
        _logger.Information("Cập nhật sản phẩm extra thành công cho thương hiệu {BrandId}", brandId);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật sản phẩm extra thành công",
            Data = productVariant.Id
        };
    }
}