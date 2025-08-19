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

namespace DimPos.Catalog.Application.Features.ComboProducts.Command.UpdateComboProduct;

public class UpdateComboProductCommandHandler : IRequestHandler<UpdateComboProductCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public UpdateComboProductCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateComboProductCommand request, CancellationToken cancellationToken)
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
            && x.Product.IsCombo
            && !x.Product.IsExtra
            && x.Product.BrandId == brandId,
            include: x => x.Include(p => p.Product)
                .ThenInclude(x => x.ProductImages)
        );
        if (productVariant == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm combo");
        }
        
        var existingMainImageCount = request.ExistComboProductImages?.Count(x => x.IsMainImage) ?? 0;
        var newMainImageCount = request.NewComboProductImages?.Count(x => x.IsMainImage) ?? 0;
        if (productVariant.Product.ProductImages != null && productVariant.Product.ProductImages.Any())
        {
            if (existingMainImageCount + newMainImageCount != 1)
            {
                throw new BadHttpRequestException("Chỉ có thể có một ảnh chính cho sản phẩm.");
            }
        }

        if (request.ExistComboProductImages?.Any() == true)
        {
            var existProductImageIds = request.ExistComboProductImages.Select(x => x.Id).ToList();
            var deleteProductImages = productVariant.Product.ProductImages?
                .Where(x => !existProductImageIds.Contains(x.Id));
            if (deleteProductImages != null) 
                _unitOfWork.GetRepository<ProductImages>().DeleteRangeAsync(deleteProductImages);
            foreach (var existImage in request.ExistComboProductImages)
            {
                var productImage = productVariant.Product.ProductImages?.FirstOrDefault(x => x.Id == existImage.Id);
                if (productImage != null)
                {
                    productImage.IsMainImage = existImage.IsMainImage;
                    productImage.AltText = existImage.AltText;
                    _unitOfWork.GetRepository<ProductImages>().UpdateAsync(productImage);
                }
            }
        }
        else
        {
            var deleteProductImages = productVariant.Product.ProductImages?.ToList();
            if (deleteProductImages != null && deleteProductImages.Any())
            {
                _unitOfWork.GetRepository<ProductImages>().DeleteRangeAsync(deleteProductImages);
            }
        }

        if (request.NewComboProductImages != null)
        {
            if (newMainImageCount != 1)
            {
                throw new BadHttpRequestException("Chỉ có thể có một ảnh chính cho sản phẩm mới.");
            }
            var uploadImageGrpcRequest = new ListImageRequest();
            var productImageList = new List<ProductImages>();
            foreach (var productImage in request.NewComboProductImages)
            {
                var productImageId = Guid.CreateVersion7();
                using var memoryStream = new MemoryStream();
                await productImage.Image.CopyToAsync(memoryStream, cancellationToken);
                var byteString = ByteString.CopyFrom(memoryStream.ToArray());
                uploadImageGrpcRequest.ImageRequest.Add(new ImageRequest()
                {
                    Id = productImageId.ToString(),
                    ChunkData = byteString
                });
                var productImageEntity = new ProductImages()
                {
                    Id = productImageId,
                    IsMainImage = productImage.IsMainImage,
                    AltText = productImage.AltText,
                    ProductId = productVariant.Product.Id
                };
                productImageList.Add(productImageEntity);
            }
            
            using var call = _mediaGrpcService.UploadImage(cancellationToken: cancellationToken);
            await call.RequestStream.WriteAsync(new UploadImageRequest()
            {
                ListImageRequest = uploadImageGrpcRequest
            }, cancellationToken);
            await call.RequestStream.CompleteAsync();
            
            var uploadImageGrpcResponse = await call.ResponseAsync;
            foreach (var imageResponse in uploadImageGrpcResponse.ListImageResponse.ImageResponse)
            {
                var productImage = productImageList.FirstOrDefault(x => x.Id.ToString() == imageResponse.Id);
                if (productImage != null)
                {
                    productImage.ImageUrl = imageResponse.ImageUrl;
                    await _unitOfWork.GetRepository<ProductImages>().InsertAsync(productImage);
                }
            }
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
        
        productVariant.IsActive = request.IsActive ?? productVariant.IsActive;
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
            throw new Exception("Cập nhật sản phẩm combo không thành công");
        }
        _logger.Information("Cập nhật sản phẩm combo thành công cho thương hiệu {BrandId}", brandId);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật sản phẩm combo thành công",
            Data = productVariant.Id
        };
    }
}