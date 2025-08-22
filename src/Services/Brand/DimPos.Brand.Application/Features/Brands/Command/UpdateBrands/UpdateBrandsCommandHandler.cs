using DimPos.Brand.Application.Services.Interface;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using Mediator;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdateBrands;

public class UpdateBrandsCommandHandler : IRequestHandler<UpdateBrandsCommand, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public UpdateBrandsCommandHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger, IClaimService claimService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateBrandsCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu hiện tại");
        }

        var brand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == brandId
        );
        
        if (brand == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu");
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
                .ImageResponse.FirstOrDefault()
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
        _logger.Information("Cập nhật thương hiệu thành công: {BrandId}", brandId);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thương hiệu thành công",
            Data = brand.Id
        };
    }
}