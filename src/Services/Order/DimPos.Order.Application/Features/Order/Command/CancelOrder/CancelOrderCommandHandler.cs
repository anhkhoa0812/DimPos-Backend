using Confluent.Kafka;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using MassTransit;
using Mediator;
using SharedProject.Events.Order.CancelOrder;

namespace DimPos.Order.Application.Features.Order.Command.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService; 
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    private readonly ITopicProducer<Null, CancelOrderResponseModel> _topicProducer;
    
    public CancelOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService, PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService,
        ITopicProducer<Null, CancelOrderResponseModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }

        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");
        }

        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.OrderId && x.StoreId == storeId && x.Status == EOrderStatus.PendingPayment
        );
        if (order == null)
        {
            throw new BadHttpRequestException("Không tìm thấy đơn hàng hoặc đơn hàng đã được xử lý");
        }
        var getCredentialsConfigGrpcResponse = await _storeGrpcService.GetCredentialsConfigBySystemPaymentMethodIdAsync(new GetCredentialsConfigBySystemPaymentMethodIdRequest()
        {
            StoreId = storeId.ToString(),
            BrandId = order.BrandId.ToString(),
            SystemPaymentMethodId = order.SystemPaymentMethodId.ToString()
        });
        if(getCredentialsConfigGrpcResponse == null || !getCredentialsConfigGrpcResponse.IsSuccess)
        {
            _logger.Error("Failed to get store credentials config for StoreId: {StoreId}", storeId);
            throw new BadHttpRequestException("Không tìm thấy cấu hình thanh toán của cửa hàng");
        }

        var checkPaymentForCancelOrder = await _paymentGrpcService.CheckForCancelOrderAsync(
            new CheckForCancelOrderRequest()
            {
                OrderId = order.Id.ToString(),
                CredentialsConfig = getCredentialsConfigGrpcResponse.CredentialsConfigAtStore,
                StoreId = storeId.ToString()
            }
        );
        if (!checkPaymentForCancelOrder.IsSuccess)
        {
            throw new BadHttpRequestException(checkPaymentForCancelOrder.Message);
        }
        var isNeedToChangeInventory = order.IsNeedToUpdateInventory;
        order.Status = EOrderStatus.Cancelled;
        order.IsNeedToUpdateInventory = false;
        order.CancellationReason = request.CancellationReason;
        order.CancelledByAccountId = accountId;
        
        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to cancel order with Id: {OrderId} for StoreId: {StoreId}", request.OrderId, storeId);
            throw new Exception("Hủy đơn hàng không thành công");
        }
        _logger.Information("Order with Id: {OrderId} has been successfully cancelled for StoreId: {StoreId}", request.OrderId, storeId);

        if (!isNeedToChangeInventory)
        {
            var cancelOrderResponseModel = new CancelOrderResponseModel
            {
                CorrelationId = Guid.CreateVersion7(),
                OrderId = order.Id,
                AccountId = accountId,
                StoreId = storeId,
            };
            await _topicProducer.Produce(
                null,
                cancelOrderResponseModel,
                cancellationToken
            ).ConfigureAwait(false);
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Hủy đơn hàng thành công",
            Data = order.Id
        };
    }
}