using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Order.Infrastructure.Utils;
using DimPos.Payment.Application.Common.Protos;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Command.UpdateCompleteOrder;

public class UpdateCompleteOrderCommandHandler : IRequestHandler<UpdateCompleteOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    public UpdateCompleteOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateCompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if(storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");

        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.OrderId && x.StoreId == storeId && x.Status == EOrderStatus.Confirmed
        );
        if (order == null)
        {
            _logger.Error("Không tìm thấy đơn hàng với ID {OrderId} hoặc đơn hàng không ở trạng thái đã xác nhận", request.OrderId);
            throw new BadHttpRequestException("Không tìm thấy đơn hàng hoặc đơn hàng không ở trạng thái đã xác nhận");
        }
        
        var checkSuccessPaymentTransaction = await _paymentGrpcService.CheckSuccessPaymentTransactionAsync(
            new CheckSuccessPaymentTransactionRequest
            {
                OrderId = request.OrderId.ToString(),
                StoreId = storeId.ToString()
            }
        );
        if (!checkSuccessPaymentTransaction.IsSuccess)
        {
            _logger.Error("Không tìm thấy giao dịch thanh toán thành công cho đơn hàng {OrderId}", request.OrderId);
            throw new BadHttpRequestException("Không tìm thấy giao dịch cho đơn hàng hoặc giao dịch không thành công");
        }
        
        order.Status = EOrderStatus.Completed;
        order.CompletedAt = TimeUtil.GetCurrentSEATime();
        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Cập nhật trạng thái đơn hàng {OrderId} không thành công", request.OrderId);
            throw new Exception("Cập nhật trạng thái đơn hàng không thành công");
        }
        _logger.Information("Cập nhật trạng thái đơn hàng {OrderId} thành công", request.OrderId);
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật trạng thái đơn hàng thành công",
            Data = order.Id
        };
    }
}