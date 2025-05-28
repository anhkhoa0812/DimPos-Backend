using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.Products.Commands.UpdateProducts;

public class UpdateProductsCommandHandler : IRequestHandler<UpdateProductsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    public UpdateProductsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
        _mediaGrpcService = mediaGrpcService;
    }
    public async ValueTask<ApiResponse> Handle(UpdateProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("BEGIN: UpdateProductsCommandHandler.Handle - ProductId: {ProductId}", request.ProductId);
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId,
            include: x => x.Include(y => y.ProductImages)
        );
        if (product == null)
            throw new NotFoundException("Không tìm thấy sản phẩm với ID đã cung cấp.");
        product.Name = request.UpdateProducts.Name ?? product.Name;
        product.Description = request.UpdateProducts.Description ?? product.Description;
        product.AlternativeCode = request.UpdateProducts.AlternativeCode ?? product.AlternativeCode;
        product.Status = request.UpdateProducts.Status ?? product.Status;
        product.IsAvailable = request.UpdateProducts.IsAvailable ?? product.IsAvailable;
        product.DisplayOrder = request.UpdateProducts.DisplayOrder ?? product.DisplayOrder;
        product.IsMenuDisplay = request.UpdateProducts.IsMenuDisplay ?? product.IsMenuDisplay;
        product.IsMostOrdered = request.UpdateProducts.IsMostOrdered ?? product.IsMostOrdered;
        product.SaleType = request.UpdateProducts.SaleType ?? product.SaleType;
        product.Note = request.UpdateProducts.Note ?? product.Note;

        var existingMainImageCount = request.UpdateProducts.ExistProductImages?.Count(x => x.IsMainImage) ?? 0;
        var newMainImageCount = request.UpdateProducts.NewProductImages?.Count(x => x.IsMainImage) ?? 0;
        if (existingMainImageCount + newMainImageCount != 1)
        {
            throw new BadHttpRequestException("Chỉ có thể có một ảnh chính cho sản phẩm.");
        }

        if (request.UpdateProducts.ExistProductImages?.Any() == true)
        {
            var existProductImageIds = request.UpdateProducts.ExistProductImages.Select(x => x.Id).ToList();
            var deleteProductImages = product.ProductImages?
                .Where(x => !existProductImageIds.Contains(x.Id));
            if (deleteProductImages != null) 
                _unitOfWork.GetRepository<ProductImages>().DeleteRangeAsync(deleteProductImages);
            foreach (var existImage in request.UpdateProducts.ExistProductImages)
            {
                var productImage = product.ProductImages?.FirstOrDefault(x => x.Id == existImage.Id);
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
            var deleteProductImages = product.ProductImages?.ToList();
            if (deleteProductImages != null && deleteProductImages.Any())
            {
                _unitOfWork.GetRepository<ProductImages>().DeleteRangeAsync(deleteProductImages);
            }
        }

        if (request.UpdateProducts.NewProductImages != null)
        {
            var uploadImageGrpcRequest = new ListImageRequest();
            var productImageList = new List<ProductImages>();
            foreach (var productImage in request.UpdateProducts.NewProductImages)
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
        _unitOfWork.GetRepository<Domain.Entities.Products>().UpdateAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information("END: UpdateProductsCommandHandler.Handle - ProductId: {ProductId}, Success: {Success}", request.ProductId, isSuccess);
        if(!isSuccess)
            throw new Exception("Cập nhật sản phẩm không thành công.");
        return new ApiResponse()
        {
            Status = 200,
            Message = "Cập nhật sản phẩm thành công.",
            Data = null
        };
    }
}