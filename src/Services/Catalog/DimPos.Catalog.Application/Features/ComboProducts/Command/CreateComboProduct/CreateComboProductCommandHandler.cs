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

namespace DimPos.Catalog.Application.Features.ComboProducts.Command.CreateComboProduct;

public class CreateComboProductCommandHandler : IRequestHandler<CreateComboProductCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public CreateComboProductCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateComboProductCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        
        var accountId = _claimService.GetCurrentUserId;
        if(accountId == Guid.Empty) 
            throw new BadHttpRequestException("Không tìm thấy id của tài khoản");
        
        var exisingProduct = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Code == request.Code
        );
        if (exisingProduct != null)
        {
            throw new BadHttpRequestException("Mã sản phẩm đã tồn tại");
        }

        if (request.ItemProductVariants.Count < 2)
        {
            throw new BadHttpRequestException("Sản phẩm combo phải có ít nhất 2 sản phẩm");
        }
        var product = new Domain.Entities.Products()
        {
            Id = Guid.CreateVersion7(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Note = request.Note,
            IsCombo = true,
            Type = EProductType.CustomerOrder,
            IsHasVariants = false,
            BrandId = brandId,
        };
        var productVariant = new Domain.Entities.ProductVariants()
        {
            Id = Guid.CreateVersion7(),
            Size = null,
            IsActive = true,
            Description = request.Description,
            Name = request.Name,
            Code = request.Code,
            Price = request.Price,
            DisplayOrder = request.DisplayOrder,
            Sku = request.Sku,
            ProductId = product.Id
        };
        product.ProductVariants.Add(productVariant);
        
        var basePrice = new BasePrice()
        {
            Id = Guid.CreateVersion7(),
            ProductVariantId = productVariant.Id,
            Price = request.Price,
            BrandId = brandId,
            CurrencyCode = "VND",
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
        
        if (request.ProductImages != null && request.ProductImages.Any())
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
        
        var itemProductVariantIds = request.ItemProductVariants.Select(x => x.ProductVariantId).ToList();
        var itemProductVariants = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetListAsync(
            predicate: x => itemProductVariantIds.Contains(x.Id)
            && x.Product.Type == EProductType.CustomerOrder
        );
        if(request.ItemProductVariants.Count != itemProductVariants.Count)
        {
            throw new BadHttpRequestException("Một hoặc nhiều sản phẩm không tồn tại");
        }
        foreach (var itemProductVariant in itemProductVariants)
        {
           var requestItemProductVariant = request.ItemProductVariants
               .First(x => x.ProductVariantId == itemProductVariant.Id);
           
           var productComboItem = new Domain.Entities.ProductComboItems()
           {
               Id = Guid.CreateVersion7(),
               DisplayOrder = requestItemProductVariant.DisplayOrder,
               Quantity = requestItemProductVariant.Quantity,
               ProductId = product.Id,
               ItemProductVariantId = itemProductVariant.Id,
           };
           await _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>().InsertAsync(productComboItem);
        }
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Tạo sản phẩm combo thất bại");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo sản phẩm combo thành công",
            Data = productVariant.Id
        };
    }
}