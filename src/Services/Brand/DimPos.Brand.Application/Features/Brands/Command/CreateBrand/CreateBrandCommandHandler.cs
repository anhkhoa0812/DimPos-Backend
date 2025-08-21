using Confluent.Kafka;
using DimPos.Brand.Application.Common.Mapper;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Application.Services.Interface;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Enums;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using DimPos.Media.Application.Common.Protos;
using Google.Protobuf;
using MassTransit;
using Mediator;
using SharedProject.Events.Brand;

namespace DimPos.Brand.Application.Features.Brands.Command.CreateBrand;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, CreateBrandAccountModel> _producer;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    public CreateBrandCommandHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger, IClaimService claimService,
        ITopicProducer<Null, CreateBrandAccountModel> producer,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    public async ValueTask<ApiResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var systemAdminAccountId = _claimService.GetCurrentUserId;
        if (systemAdminAccountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản hiện tại");
        }
        var existingBrand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Code == request.Code
        );
        if (existingBrand != null)
        {
            throw new BadHttpRequestException("Mã thương hiệu đã tồn tại");
        }
        var brand = BrandMapper.ToBrands(request);
        brand.Id = Guid.CreateVersion7();
        brand.Status = EBrandStatus.Active;
        
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
                .FirstOrDefault(x => x.Id == imageRequestId)
                ?.ImageUrl;
            if (string.IsNullOrEmpty(imageResponse))
                throw new Exception("Lỗi khi tải ảnh lên");
            brand.PictureUrl = imageResponse;
        }
        
        await _unitOfWork.GetRepository<Domain.Entities.Brands>().InsertAsync(brand);

        var accountId = Guid.CreateVersion7();
        var brandAccount = new BrandAccounts()
        {
            Id = Guid.CreateVersion7(),
            BrandId = brand.Id,
            AccountId = accountId
        };
        await _unitOfWork.GetRepository<BrandAccounts>().InsertAsync(brandAccount);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Không thể tạo mới thương hiệu");
        }
        
        var (hashPassword, saltPassword) = PasswordUtil.HashPassword(request.Password);
        var createBrandAccountModel = new CreateBrandAccountModel()
        {
            CorrelationId = Guid.CreateVersion7(),
            SystemAdminAccountId = systemAdminAccountId,
            BrandId = brand.Id,
            AccountId = accountId,
            Code = brand.Code,
            Email = brand.Email,
            Username = request.Username,
            HashPassword = hashPassword,
            SaltPassword = saltPassword,
        };
        await _producer.Produce(
            key: null,
            createBrandAccountModel,
            cancellationToken: cancellationToken
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo mới thương hiệu thành công",
        };
        
    }
}