using System.Text.Json;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.MPos;
using DimPos.Store.Domain.Models.PayOs;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.CreateStorePaymentMethodConfig;

public class CreateStorePaymentMethodConfigCommandHandler : IRequestHandler<CreateStorePaymentMethodConfigCommand, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    
    public CreateStorePaymentMethodConfigCommandHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, 
        IClaimService claimService, PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateStorePaymentMethodConfigCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var existingStorePaymentMethodConfig = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>()
            .SingleOrDefaultAsync(
                predicate: x => x.StoreId == storeId && x.SystemPaymentMethodTypeId == request.SystemPaymentMethodId
            );
        if (existingStorePaymentMethodConfig != null)
        {
            throw new BadHttpRequestException("Cấu hình phương thức thanh toán đã tồn tại cho cửa hàng này");
        }

        var systemPaymentMethodGrpc = await _paymentGrpcService.GetSystemPaymentMethodByIdAsync(
            new GetSystemPaymentMethodByIdRequest() 
            {
                Id = request.SystemPaymentMethodId.ToString()
            }
        );
        if (systemPaymentMethodGrpc == null)
        {
            throw new BadHttpRequestException("Không tìm thấy phương thức thanh toán hệ thống");
        }
        var storePaymentMethodConfig = new StorePaymentMethodConfigs()
        {
            Id = Guid.CreateVersion7(),
            StoreId = storeId,
            SystemPaymentMethodTypeId = Guid.Parse(systemPaymentMethodGrpc.Id),
            IsActiveByStore = true,
        };
        if (systemPaymentMethodGrpc.PaymentMethod == PaymentMethod.CardEdc ||
            systemPaymentMethodGrpc.PaymentMethod == PaymentMethod.QrEdc ||
            systemPaymentMethodGrpc.PaymentMethod == PaymentMethod.QrVietqr)
        {
            if (string.IsNullOrEmpty(request.CredentialsConfigAtStore))
            {
                throw new BadHttpRequestException("Cấu hình thông tin xác thực không được để trống cho phương thức thanh toán này");
            }

            var mPosModelRequest = JsonSerializer.Deserialize<MPosModelRequest>(request.CredentialsConfigAtStore);
            if (mPosModelRequest == null)
            {
                throw new BadHttpRequestException("Thông tin xác thực không hợp lệ");
            }
            var data = CryptographyUtil.EncodeCredentialsConfig(JsonSerializer.Serialize(mPosModelRequest.Settings), storeId.ToString("N"));
            storePaymentMethodConfig.CredentialsConfigAtStore =
                JsonSerializer.Serialize(new MPosModel()
                {
                    MerchantId = mPosModelRequest.MerchantId,
                    Data = data
                });
        }
        else if (systemPaymentMethodGrpc.PaymentMethod == PaymentMethod.QrPayos)
        {
            if (string.IsNullOrEmpty(request.CredentialsConfigAtStore))
            {
                throw new BadHttpRequestException("Cấu hình thông tin xác thực không được để trống cho phương thức thanh toán này");
            }

            var payOsModel = JsonSerializer.Deserialize<PayOsModel>(request.CredentialsConfigAtStore);
            if (payOsModel == null)
            {
                throw new BadHttpRequestException("Thông tin xác thực không hợp lệ");
            }
            var data = CryptographyUtil.EncodeCredentialsConfig(JsonSerializer.Serialize(payOsModel), storeId.ToString("N"));
            storePaymentMethodConfig.CredentialsConfigAtStore = data;
        }
        await _unitOfWork.GetRepository<StorePaymentMethodConfigs>().InsertAsync(storePaymentMethodConfig);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Không thể tạo cấu hình phương thức thanh toán cho cửa hàng");
            throw new Exception("Không thể tạo cấu hình phương thức thanh toán cho cửa hàng");
        }
        _logger.Information("Tạo cấu hình phương thức thanh toán cho cửa hàng thành công");
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo cấu hình phương thức thanh toán cho cửa hàng thành công",
            Data = null
        };
    }
}