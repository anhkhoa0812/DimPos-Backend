using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommandHandler : IRequestHandler<CreateProductsCommand, ApiResponse>
{
    
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IUploadService _uploadService;
    private readonly IClaimService _claimService;
    public CreateProductsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork,
        ILogger logger, IUploadService uploadService, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _uploadService = uploadService;
        _claimService = claimService;
    }
    public async ValueTask<ApiResponse> Handle(CreateProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(CreateProductsCommandHandler)} - {DateTime.UtcNow}");
        
        var product = ProductMapper.ToProducts(request);
        product.Id = Guid.CreateVersion7();
        product.IsMenuDisplay = false;
        product.IsMostOrdered = false;
        product.ProductVariants = new List<Domain.Entities.ProductVariants>();
        product.BrandId = _claimService.GetBrandId;
        if (request.ProductVariants != null)
        {
            foreach (var productVariant in request.ProductVariants)
            {
                var productVariants = ProductVariantMapper.ToPoProductVariants(productVariant);
                productVariants.Id = Guid.CreateVersion7();
                productVariants.ProductId = product.Id;
                productVariants.IsActive = false;
                productVariants.IsMenuDisplay = false;
                product.ProductVariants.Add(productVariants);
            }
            product.IsHasVariants = true;
        }
        else
        {
            if (request.Price == null)
            {
                return new ApiResponse()
                {
                    Status = (int) HttpStatusCode.BadRequest,
                    Message = "Giá sản phẩm không được để trống",
                };
            }
            product.IsHasVariants = false;
            //Chưa set Status
            var productVariant = new Domain.Entities.ProductVariants()
            {
                Id = Guid.CreateVersion7(),
                IsMenuDisplay = false,
                Code = request.Code,
                Name = request.Name,
                AlternativeCode = request.AlternativeCode,
                ProductId = product.Id,
                Price = request.Price,
                PriceCOGS = request.PriceCOGS,
                IsActive = false,
                DiscountPercent = request.DiscountPercent,
                DiscountPrice = request.DiscountPrice,
                DisplayOrder = request.DisplayOrder,
            };
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

            //Lưu ảnh xuống local VPS, publish 1 event đến uploadService, để upload ảnh lên S3 và cập nhập lại data
            await Parallel.ForEachAsync(request.ProductImages, cancellationToken, async (productImage, ct) =>
            {
                if(productImage.Image == null)
                    throw new BadHttpRequestException("Hình ảnh không được để trống");
                var entity = new ProductImages()
                {
                    Id = Guid.CreateVersion7(),
                    IsMainImage = productImage.IsMainImage,
                    AltText = productImage.AltText,
                    ProductId = product.Id
                };
                
                var url = await _uploadService.UploadImageAsync(productImage.Image);
                if (!string.IsNullOrEmpty(url))
                    entity.ImageUrl = url;
                await _unitOfWork.GetRepository<ProductImages>().InsertAsync(entity);
            });
        }
        await _unitOfWork.GetRepository<Domain.Entities.Products>().InsertAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateProductsCommandHandler)} - {DateTime.UtcNow}");
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