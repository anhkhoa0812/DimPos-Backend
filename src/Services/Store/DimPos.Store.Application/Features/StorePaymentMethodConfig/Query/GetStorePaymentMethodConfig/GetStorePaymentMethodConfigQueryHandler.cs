using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Store.Application.Features.StorePaymentMethodConfig.Query.GetStorePaymentMethodConfig;

public class GetStorePaymentMethodConfigQueryHandler : IRequestHandler<GetStorePaymentMethodConfigQuery, ApiResponse>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    
    public GetStorePaymentMethodConfigQueryHandler(IUnitOfWork<StoreContext> unitOfWork, ILogger logger, IClaimService claimService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePaymentMethodConfigQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        var storePaymentMethodConfigs = await _unitOfWork.GetRepository<StorePaymentMethodConfigs>().GetListAsync(
            predicate: x => x.StoreId == storeId,
            orderBy: x => x.OrderBy(x => x.CreatedDate)
        );
        if (storePaymentMethodConfigs == null || !storePaymentMethodConfigs.Any())
        {
            return new ApiResponse
            {
                Status = StatusCodes.Status200OK,
                Message = "Không tìm thấy cấu hình phương thức thanh toán cho cửa hàng này.",
                Data = new List<GetStorePaymentMethodConfigResponse>()
            };
        }
        
        var systemPaymentMethods = await _paymentGrpcService.GetSystemPaymentMethodListByIdAsync(
            new GetSystemPaymentMethodListByIdRequest()
            {
                SystemPaymentMethodIds = { storePaymentMethodConfigs.Select(x => x.SystemPaymentMethodTypeId.ToString()) }
            }
        );

        var response = new List<GetStorePaymentMethodConfigResponse>();

        foreach (var systemPaymentMethod in systemPaymentMethods.SystemPaymentMethods)
        {
            var storePaymentMethodConfig = storePaymentMethodConfigs
                .FirstOrDefault(x => x.SystemPaymentMethodTypeId.ToString() == systemPaymentMethod.Id);
            if (storePaymentMethodConfig != null)
            {
                response.Add(new GetStorePaymentMethodConfigResponse()
                {
                    Id = storePaymentMethodConfig.Id,
                    SystemPaymentMethodId = storePaymentMethodConfig.SystemPaymentMethodTypeId,
                    Name = systemPaymentMethod.Name,
                    Code = systemPaymentMethod.Code,
                    Description = systemPaymentMethod.Description,
                    IsActiveByStore = storePaymentMethodConfig.IsActiveByStore,
                    CreatedDate = storePaymentMethodConfig.CreatedDate,
                    LastModifiedDate = storePaymentMethodConfig.LastModifiedDate,
                    PaymentMethod = (EPaymentMethod) systemPaymentMethod.PaymentMethod,
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