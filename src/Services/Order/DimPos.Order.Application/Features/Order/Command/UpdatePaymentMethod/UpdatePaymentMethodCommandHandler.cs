using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;

public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    public UpdatePaymentMethodCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger,
        IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("BEGIN: UpdatePaymentMethodCommandHandler.Handle");
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }
        
        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.OrderId && x.StoreId == storeId
        );
        if (order == null)
        {
            throw new BadHttpRequestException("Không tìm thấy đơn hàng với ID đã cung cấp.");
        }

        if (order.Status != EOrderStatus.PendingPayment)
        {
            throw new BadHttpRequestException("Chỉ có thể cập nhật phương thức thanh toán cho đơn hàng đang chờ thanh toán.");
        }
        var storeDetailGrpcResponse = await _storeGrpcService.GetTaxRateAndPaymentMethodConfigAsync(new GetTaxRateAndPaymentMethodConfigRequest()
        {
            StoreId = storeId.ToString(),
            BrandId = order.BrandId.ToString(),
            StorePaymentMethodConfigId = request.StorePaymentMethodConfigId.ToString()
        });

        var updatePaymentMethodResponse = await _paymentGrpcService.UpdatePaymentMethodForOrderAsync(
            new UpdatePaymentMethodForOrderRequest()
            {
                OrderId = order.Id.ToString(),
                StoreId = storeId.ToString(),
                CredentialsConfig = storeDetailGrpcResponse.CredentialsConfigAtStore,
                SystemPaymentMethodId = storeDetailGrpcResponse.SystemPaymentMethodId,
                Amount = (float)order.TotalAmount,
            });
        
        order.SystemPaymentMethodNameSnapshot = updatePaymentMethodResponse.SystemPaymentMethodName;

        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
        _logger.Information("END: UpdatePaymentMethodCommandHandler.Handle");
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Cập nhật phương thức thanh toán không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật phương thức thanh toán thành công",
            Data = updatePaymentMethodResponse.QrLink 
        };
    }
}