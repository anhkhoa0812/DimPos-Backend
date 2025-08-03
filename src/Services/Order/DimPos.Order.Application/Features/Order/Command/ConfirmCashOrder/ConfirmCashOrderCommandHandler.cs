using Confluent.Kafka;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using MassTransit;
using Mediator;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Order.Application.Features.Order.Command.ConfirmCashOrder;

public class ConfirmCashOrderCommandHandler : IRequestHandler<ConfirmCashOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    private readonly ITopicProducer<Null, ConfirmForCashOrderResponseModel> _topicProducer;
    public ConfirmCashOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger,
        IClaimService claimService, PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService,
        ITopicProducer<Null, ConfirmForCashOrderResponseModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(ConfirmCashOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.OrderId 
                            && x.StoreId == storeId 
                            && x.Status == EOrderStatus.PendingPayment
        );
        if (order == null)
        {
            throw new BadHttpRequestException("Không tìm thấy đơn hàng hoặc đơn hàng không ở trạng thái chờ thanh toán");
        }
        
        if (request.AmountPaid < order.TotalAmount)
        {
            throw new BadHttpRequestException("Số tiền thanh toán không đủ");
        }

        var paymentTransactionGrpc = await _paymentGrpcService.GetPaymentTransactionByOrderIdAsync(
            new GetPaymentTransactionByOrderIdRequest()
            {
                OrderId = order.Id.ToString(),
                StoreId = storeId.ToString()
            }
        );
        if (paymentTransactionGrpc.PaymentMethod != PaymentMethod.Cash)
        {
            throw new BadHttpRequestException("Phương thức thanh toán không phải tiền mặt");
        }
        
        order.Status = EOrderStatus.Confirmed;
        order.AmountPaid = request.AmountPaid;
        order.CashRoundingAmount = request.AmountPaid - order.TotalAmount;

        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Xác nhận đơn hàng thất bại: {OrderId}", order.Id);
            throw new BadHttpRequestException("Xác nhận đơn hàng thất bại");
        }
        var confirmForCashOrderResponseModel = new ConfirmForCashOrderResponseModel()
        {
            CorrelationId = Guid.CreateVersion7(),
            OrderId = order.Id,
            StoreId = storeId,
            PaymentTransactionId = Guid.Parse(paymentTransactionGrpc.Id)
        };
        await _topicProducer.Produce(
            null,
            confirmForCashOrderResponseModel,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        
        _logger.Information("Xác nhận đơn hàng thành công: {OrderId}", order.Id);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xác nhận đơn hàng thành công",
            Data = order.Id
        };
    }
}