using System.Net;
using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Command.UpdateCategories;

public class UpdateCategoriesCommandHandler : IRequestHandler<UpdateCategoriesCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public UpdateCategoriesCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
        
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateCategoriesCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var category = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CategoryId && x.BrandId == brandId
        );
        category.Name = request.Name;
        category.Description = request.Description;
        category.DisplayOrder = request.DisplayOrder;
        category.Status = request.Status;

        if (category.Type == ECategoryType.Child && request.ParentCategoryId != Guid.Empty)
        {
            if(category.ParentId != request.ParentCategoryId)
            {
                var parentCategory = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
                    predicate: x => x.Id == request.ParentCategoryId && x.BrandId == brandId
                );
                if (parentCategory == null)
                    throw new NotFoundException("Không tìm thấy danh mục cha");
                category.ParentId = parentCategory.Id;
                
                parentCategory.HasChildCategory = true;
                _unitOfWork.GetRepository<Domain.Entities.Categories>().UpdateAsync(parentCategory);
            }
        }

        if (request.Image != null)
        {
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream, cancellationToken);
            var byteString = ByteString.CopyFrom(memoryStream.ToArray());
            var imageRequestId = Guid.CreateVersion7().ToString();
            var imageRequest = new ImageRequest()
            {
                Id = imageRequestId,
                ChunkData = byteString
            };
            using var call = _mediaGrpcService.UploadImage(cancellationToken: cancellationToken);
            await call.RequestStream.WriteAsync(new UploadImageRequest()
            {
                ListImageRequest = new ListImageRequest()
                {
                    ImageRequest = { imageRequest }
                }
            });
            await call.RequestStream.CompleteAsync();
            
            var uploadImageGrpcResponse = await call.ResponseAsync;
            var imageResponse = uploadImageGrpcResponse.ListImageResponse
                .ImageResponse
                .FirstOrDefault(x => x.Id == imageRequestId)
                ?.ImageUrl;
            if (string.IsNullOrEmpty(imageResponse))
                throw new Exception("Lỗi khi tải ảnh lên");
            category.PictureUrl = imageResponse;
        }
        _unitOfWork.GetRepository<Domain.Entities.Categories>().UpdateAsync(category);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if(!isSuccess)
            throw new Exception("Lỗi khi cập nhật danh mục");
        return new ApiResponse()
        {
            Status = (int)HttpStatusCode.OK,
            Message = "Cập nhập dữ liệu thành công",
            Data = null
        };
    }
}