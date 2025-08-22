using System.Net;
using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;

public class CreateCategoriesCommandHandler : IRequestHandler<CreateCategoriesCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    public CreateCategoriesCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateCategoriesCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        _logger.Information($"BEGIN: {nameof(CreateCategoriesCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        if (request.Code != null)
        {
            var existingCategory = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
                predicate: c => c.Code == request.Code && c.BrandId == brandId
            );
            if (existingCategory != null)
            {
                throw new BadHttpRequestException("Mã danh mục đã tồn tại");
            }
        }
        var category = CategoriesMapper.ToCategories(request);
        category.Id = Guid.CreateVersion7();
        category.HasChildCategory = false;
        category.BrandId = brandId;
        if (request.Type == ECategoryType.Child)
        {
            if(request.ParentId == Guid.Empty)
                throw new BadHttpRequestException("Danh mục cha không được để trống");
            var parentCategory = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
                predicate: c => c.Id == request.ParentId
            );
            if (parentCategory == null)
            {
                throw new NotFoundException("Không tìm thấy danh mục cha");
            }
            category.ParentId = parentCategory.Id;
            parentCategory.HasChildCategory = true;
            _unitOfWork.GetRepository<Domain.Entities.Categories>().UpdateAsync(parentCategory);
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
                .FirstOrDefault()
                ?.ImageUrl;
            if (string.IsNullOrEmpty(imageResponse))
                throw new Exception("Lỗi khi tải ảnh lên");
            category.PictureUrl = imageResponse;
        }
        await _unitOfWork.GetRepository<Domain.Entities.Categories>().InsertAsync(category); 
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateCategoriesCommandHandler)} - {TimeUtil.GetCurrentSEATime()}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = (int) HttpStatusCode.Created,
                Message = "Tạo danh mục thành công",
            };
        }
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.InternalServerError,
            Message = "Tạo danh mục thất bại",
            Data = category.Id
        };
    }
}