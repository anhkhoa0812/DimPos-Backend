using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Query.GetStorePaymentMethodConfigForPos;

public class GetStorePaymentMethodConfigForPosQueryHandler : IRequestHandler<GetStorePaymentMethodConfigForPosQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    
    public GetStorePaymentMethodConfigForPosQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePaymentMethodConfigForPosQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }
        var availableStorePaymentMethodConfigs = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>()
            .GetListAsync(
                predicate: x => x.StoreId == storeId && x.IsActiveByStore
            );
        var systemPaymentMethods = await _paymentGrpcService.GetSystemPaymentMethodListByIdAsync(
            new GetSystemPaymentMethodListByIdRequest()
            {
                SystemPaymentMethodIds = { availableStorePaymentMethodConfigs.Select(x => x.SystemPaymentMethodTypeId.ToString()) }
            }
        );
        var response = new List<GetStorePaymentMethodConfigForPosResponse>();

        foreach (var availableStorePaymentMethodConfig in availableStorePaymentMethodConfigs)
        {
            var systemPaymentMethod = systemPaymentMethods.SystemPaymentMethods
                .FirstOrDefault(x => x.Id == availableStorePaymentMethodConfig.SystemPaymentMethodTypeId.ToString());
            if (systemPaymentMethod != null)
            {
                response.Add(new GetStorePaymentMethodConfigForPosResponse()
                {
                    Id = availableStorePaymentMethodConfig.Id,
                    SystemPaymentMethodId = availableStorePaymentMethodConfig.SystemPaymentMethodTypeId,
                    Name = systemPaymentMethod.Name,
                    Code = systemPaymentMethod.Code,
                    Description = systemPaymentMethod.Description,
                    PaymentMethod = (EPaymentMethod) systemPaymentMethod.PaymentMethod
                });
            }
        }
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách cấu hình phương thức thanh toán cửa hàng thành công",
            Data = response
        };
    }
}