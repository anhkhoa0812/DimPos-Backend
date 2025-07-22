using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
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

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommandHandler : IRequestHandler<CreateProductsCommand, ApiResponse>
{
    
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    public CreateProductsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork,
        ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
        _mediaGrpcService = mediaGrpcService;
    }
    public async ValueTask<ApiResponse> Handle(CreateProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(CreateProductsCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }
        
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy accountId");
        }
        var existingProduct = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Code == request.Code
        );
        if (existingProduct != null)
        {
            throw new BadHttpRequestException($"Mã {request.Code} đã tồn tại");
        }
        var product = ProductMapper.ToProducts(request);
        product.Id = Guid.CreateVersion7();
        product.ProductVariants = new List<Domain.Entities.ProductVariants>();
        product.BrandId = brandId;
        product.Status = EProductStatus.Active;
        product.Type = EProductType.CustomerOrder;
        product.IsCombo = false;
        var category = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CategoryId
        );
        if (category == null)
        {
            throw new BadHttpRequestException("Không tìm thấy danh mục");
        }

        switch (category.Type)
        {
            case ECategoryType.Parent:
                if(category.HasChildCategory) 
                    throw new BadHttpRequestException("Không thể thêm sản phẩm vào danh mục này");
                product.CategoryId = category.Id;
                break;
            case ECategoryType.Child:
                product.CategoryId = category.Id;
                break;
            default:
                throw new BadHttpRequestException("Không thể thêm sản phẩm vào danh mục này");
                
        }
        
        if (request.ProductVariants != null)
        {
            foreach (var productVariant in request.ProductVariants)
            {
                var existingProductVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
                    predicate: x => x.Code == productVariant.Code
                );
                if (existingProductVariant != null)
                {
                    throw new BadHttpRequestException($"Mã {productVariant.Code} đã tồn tại");
                }
                var productVariants = ProductVariantMapper.ToPoProductVariants(productVariant);
                productVariants.Id = Guid.CreateVersion7();
                productVariants.ProductId = product.Id;
                productVariants.IsActive = false;
                productVariants.Price = productVariant.BrandPrice;
                productVariants.Description = productVariant.Description;
                product.ProductVariants.Add(productVariants);

                var basePrice = new BasePrice()
                {
                    Id = Guid.CreateVersion7(),
                    ProductVariantId = productVariants.Id,
                    Price = productVariant.BrandPrice,
                    BrandId = brandId,
                    CurrencyCode = "VND",
                    BrandPriceHistories = new List<BrandPriceHistory>()
                    {
                        new BrandPriceHistory()
                        {
                            Id = Guid.CreateVersion7(),
                            OldPrice = 0,
                            NewPrice = productVariant.BrandPrice,
                            ChangedAt = TimeUtil.GetCurrentSEATime(),
                            ChangedBy = accountId,
                            ProductVariantId = productVariants.Id,
                            CurrencyCode = "VND"
                        }
                    }
                };
                await _unitOfWork.GetRepository<BasePrice>().InsertAsync(basePrice);
            }
            
            product.IsHasVariants = true;
        }
        else
        {
            if(request.Price == null)
            {
                throw new BadHttpRequestException("Giá không được để trống");
            }
            product.IsHasVariants = false;
            //Chưa set Status
            var productVariant = new Domain.Entities.ProductVariants()
            {
                Id = Guid.CreateVersion7(),
                Code = request.Code,
                Name = request.Name,
                ProductId = product.Id,
                Price = request.Price ?? 0,
                IsActive = false,
                DisplayOrder = request.DisplayOrder,
                Description = request.Description,
                Size = null,
                Sku = request.Sku
            };
            var basePrice = new BasePrice()
            {
                Id = Guid.CreateVersion7(),
                ProductVariantId = productVariant.Id,
                Price = request.Price ?? 0,
                BrandId = brandId,
                CurrencyCode = "VND",
                BrandPriceHistories = new List<BrandPriceHistory>()
                {
                    new BrandPriceHistory()
                    {
                        Id = Guid.CreateVersion7(),
                        OldPrice = 0,
                        NewPrice = request.Price ?? 0,
                        ChangedAt = TimeUtil.GetCurrentSEATime(),
                        ChangedBy = brandId,
                        CurrencyCode = "VND",
                        ProductVariantId = productVariant.Id
                    }
                }
            };
            await _unitOfWork.GetRepository<BasePrice>().InsertAsync(basePrice);
            product.ProductVariants.Add(productVariant);
        }
        if (request.ModifierGroupIds != null)
        {
            foreach (var modifierGroupId in request.ModifierGroupIds)
            {
                var modifierGroup = await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().SingleOrDefaultAsync(
                    predicate: x => x.Id == modifierGroupId
                );
                if (modifierGroup != null)
                {
                    var productModifierGroup = new ProductModifierGroups()
                    {
                        Id = Guid.CreateVersion7(),
                        ProductId = product.Id,
                        ModifierGroupId = modifierGroup.Id
                    };
                    await _unitOfWork.GetRepository<ProductModifierGroups>().InsertAsync(productModifierGroup);
                }
            }
        }

        if (request.ProductImages != null)
        {
            if (request.ProductImages.Count(x => x.IsMainImage) != 1)
            {
                throw new BadHttpRequestException("Chỉ được chọn 1 ảnh chính");
            }

            var uploadImageGrpcRequest = new ListImageRequest();
            var productImageList = new List<ProductImages>();
            foreach (var productImage in request.ProductImages)
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
                    ProductId = product.Id
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
                var productImage = productImageList.First(x => x.Id.ToString() == imageResponse.Id);
                productImage.ImageUrl = imageResponse.ImageUrl;
            }
            product.ProductImages = productImageList;
            // //Lưu ảnh xuống local VPS, publish 1 event đến uploadService, để upload ảnh lên S3 và cập nhập lại data
            // await Parallel.ForEachAsync(request.ProductImages, cancellationToken, async (productImage, ct) =>
            // {
            //     if(productImage.Image == null)
            //         throw new BadHttpRequestException("Hình ảnh không được để trống");
            //     var entity = new ProductImages()
            //     {
            //         Id = Guid.CreateVersion7(),
            //         IsMainImage = productImage.IsMainImage,
            //         AltText = productImage.AltText,
            //         ProductId = product.Id
            //     };
            //     
            //     var url = await _uploadService.UploadImageAsync(productImage.Image);
            //     if (!string.IsNullOrEmpty(url))
            //         entity.ImageUrl = url;
            //     await _unitOfWork.GetRepository<ProductImages>().InsertAsync(entity);
            // });
        }
        await _unitOfWork.GetRepository<Domain.Entities.Products>().InsertAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateProductsCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = (int) HttpStatusCode.Created,
                Message = "Create product successfully",
                Data = ProductMapper.ToProductResponse(product)
            };
        }
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.InternalServerError,
            Message = "Create product failed",
        };
    }
}