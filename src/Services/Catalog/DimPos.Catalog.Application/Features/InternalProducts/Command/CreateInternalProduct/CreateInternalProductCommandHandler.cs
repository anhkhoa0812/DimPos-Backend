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

namespace DimPos.Catalog.Application.Features.InternalProducts.Command.CreateInternalProduct;

public class CreateInternalProductCommandHandler : IRequestHandler<CreateInternalProductCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public CreateInternalProductCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, 
        IClaimService claimService, MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateInternalProductCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");
        var exisingProduct = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Code == request.Code
        );
        if (exisingProduct != null)
        {
            throw new BadHttpRequestException("Mã sản phẩm đã tồn tại");
        }
        
        var exisingProductVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Code == request.Code
        );
        if (exisingProductVariant != null)
        {
            throw new BadHttpRequestException("Mã sản phẩm đã tồn tại");
        }
        _logger.Information($"BEGIN: {nameof(CreateInternalProductCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        var product = new Domain.Entities.Products()
        {
            Id = Guid.CreateVersion7(),
            BrandId = brandId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Note = request.Note,
            IsHasVariants = false,
            Type = EProductType.InternalOrder,
            Status = EProductStatus.Active,
        };
        
        var productVariant = new Domain.Entities.ProductVariants()
        {
            Id = Guid.CreateVersion7(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            Price = request.Price,
            Size = null,
            Sku = request.Sku,
            ProductId = product.Id,
        };
        product.ProductVariants.Add(productVariant);
        
        var basePrice = new BasePrice()
        {
            Id = Guid.CreateVersion7(),
            ProductVariantId = productVariant.Id,
            Price = request.Price,
            BrandId = brandId,
            CurrencyCode = "VND",
            EffectiveFrom = TimeUtil.GetCurrentSEATime(),
            BrandPriceHistories = new List<BrandPriceHistory>()
            {
                new BrandPriceHistory()
                {
                    Id = Guid.CreateVersion7(),
                    OldPrice = 0,
                    NewPrice = request.Price,
                    ChangedAt = TimeUtil.GetCurrentSEATime(),
                    ChangedBy = accountId,
                    ProductVariantId = productVariant.Id,
                    CurrencyCode = "VND"
                }
            }
        };
        await _unitOfWork.GetRepository<BasePrice>().InsertAsync(basePrice);

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
                var productImage = productImageList.FirstOrDefault(x => x.Id.ToString() == imageResponse.Id);
                if (productImage != null)
                {
                    productImage.ImageUrl = imageResponse.ImageUrl;
                    await _unitOfWork.GetRepository<ProductImages>().InsertAsync(productImage);
                }
            }
        }
        await _unitOfWork.GetRepository<Domain.Entities.Products>().InsertAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateInternalProductCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        if (!isSuccess)
        {
            return new ApiResponse()
            {
                Status = StatusCodes.Status500InternalServerError,
                Message = "Tạo sản phẩm nội bộ thất bại",
            };
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo sản phẩm nội bộ thành công",
        };
    }
}