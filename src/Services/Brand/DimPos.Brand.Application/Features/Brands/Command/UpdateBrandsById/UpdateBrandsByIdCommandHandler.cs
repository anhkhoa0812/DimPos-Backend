using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdateBrandsById;

public class UpdateBrandsByIdCommandHandler : IRequestHandler<UpdateBrandsByIdCommand, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public UpdateBrandsByIdCommandHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateBrandsByIdCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.BrandId
        );
        if (brand == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu với Id đã cung cấp");
        }
        
        brand.Name = request.Name ?? brand.Name;
        brand.Address = request.Address ?? brand.Address;
        brand.Phone = request.Phone ?? brand.Phone;

        if (request.Picture != null)
        {
            using var memoryStream = new MemoryStream();
            await request.Picture.CopyToAsync(memoryStream, cancellationToken);
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
            brand.PictureUrl = imageResponse;
        }
        
        _unitOfWork.GetRepository<Domain.Entities.Brands>().UpdateAsync(brand);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Cập nhật thương hiệu không thành công");
        }
        _logger.Information("Cập nhật thương hiệu thành công: {BrandId}", brand.Id);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thương hiệu thành công",
            Data = brand.Id
        };
    }
}