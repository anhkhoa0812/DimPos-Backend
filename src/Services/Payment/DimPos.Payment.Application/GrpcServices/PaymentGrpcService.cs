using DimPos.Payment.Application.Common.Protos;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Domain.Models.Payment;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Grpc.Core;

namespace DimPos.Payment.Application.GrpcServices;

public class PaymentGrpcService : Common.Protos.PaymentGrpcService.PaymentGrpcServiceBase
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IMPosService _mPosService;
    public PaymentGrpcService(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger, IMPosService mPosService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mPosService = mPosService;
    }
    public override async Task<CreateB2CPaymentResponse> CreateB2CPayment(CreateB2CPaymentRequest request, ServerCallContext context)
    {
        var paymentMethodType = (ESystemPaymentMethod) request.PaymentMethod;
        var systemPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Type == paymentMethodType
        );
        if (systemPaymentMethod == null)
        {
            _logger.Error($"System payment method not found for type: {paymentMethodType}");
            return new CreateB2CPaymentResponse()
            {
                IsSuccess = false,
                Message = "System payment method not found",
                PaymentUrl = String.Empty
            };
        }

        var paymentTransaction = new PaymentTransactions()
        {
            Id = Guid.CreateVersion7(),
            OrderId = Guid.Parse(request.OrderId),
            StoreId = Guid.Parse(request.StoreId),
            CustomerId = request.CustomerId != null ? Guid.Parse(request.CustomerId) : Guid.Empty,
            Amount = Decimal.Parse(request.Amount),
            CurrencyCode = request.CurrencyCode,
            Description = request.Description != null ? request.Description : String.Empty,
            TransactionTime = DateTime.UtcNow,
            SystemPaymentMethodTypeId = systemPaymentMethod.Id
        };
        string? qrPaymentUrl = String.Empty;
        switch (systemPaymentMethod.Type)
        {
            case ESystemPaymentMethod.CASH:
                paymentTransaction.TransactionType = ETransactionType.SALE_CAPTURE_B2C;
                paymentTransaction.Status = EPaymentTransactionStatus.SUCCESS;
                break;
            case ESystemPaymentMethod.QR_VIETQR:
                var createQrPaymentRequest = new CreateQrPaymentRequest()
                {
                    Amount = Double.Parse(request.Amount),
                    Description = request.Description
                };
                qrPaymentUrl = await _mPosService.CreateQr(createQrPaymentRequest);
                paymentTransaction.TransactionType = ETransactionType.SALE_CAPTURE_B2C;
                paymentTransaction.Status = EPaymentTransactionStatus.PENDING;
                break;
            case ESystemPaymentMethod.QR_EDC:
                var createEDCQrPaymentRequest = new CreateEDCPaymentRequest()
                {
                    Amount = Decimal.Parse(request.Amount),
                    Description = request.Description,
                    PaymentMethod = EEDCPaymentMethod.QR
                };
                var createEDCQrPaymentResponse = await _mPosService.CreateEDCPayment(createEDCQrPaymentRequest);
                if (createEDCQrPaymentResponse == null)
                {
                    _logger.Error("Failed to create EDC payment");
                    return new CreateB2CPaymentResponse()
                    {
                        IsSuccess = false,
                        Message = "Failed to create EDC payment",
                        PaymentUrl = String.Empty
                    };
                }
                paymentTransaction.TransactionType = ETransactionType.SALE_CAPTURE_B2C;
                paymentTransaction.Status = EPaymentTransactionStatus.PENDING;
                break;
            case ESystemPaymentMethod.CARD_EDC:
                var createEDCCardPaymentRequest = new CreateEDCPaymentRequest()
                {
                    Amount = Decimal.Parse(request.Amount),
                    Description = request.Description,
                    PaymentMethod = EEDCPaymentMethod.QR
                };
                var createEDCCardPaymentResponse = await _mPosService.CreateEDCPayment(createEDCCardPaymentRequest);
                if (createEDCCardPaymentResponse == null)
                {
                    _logger.Error("Failed to create EDC payment");
                    return new CreateB2CPaymentResponse()
                    {
                        IsSuccess = false,
                        Message = "Failed to create EDC payment",
                        PaymentUrl = String.Empty
                    };
                }
                paymentTransaction.TransactionType = ETransactionType.SALE_CAPTURE_B2C;
                paymentTransaction.Status = EPaymentTransactionStatus.PENDING;
                break;
            default:
                return new CreateB2CPaymentResponse()
                {
                    IsSuccess = false,
                    Message = "Unsupported payment method",
                    PaymentUrl = String.Empty
                };
        }
        await _unitOfWork.GetRepository<PaymentTransactions>().InsertAsync(paymentTransaction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to create payment transaction");
            return new CreateB2CPaymentResponse()
            {
                IsSuccess = false,
                Message = "Failed to create payment transaction",
                PaymentUrl = String.Empty
            };
        }
        return new CreateB2CPaymentResponse()
        {
            IsSuccess = true,
            Message = "Payment transaction created successfully",
            PaymentUrl = qrPaymentUrl
        };
    }

    public override async Task<GetSystemPaymentMethodByIdResponse> GetSystemPaymentMethodById(GetSystemPaymentMethodByIdRequest request, ServerCallContext context)
    {
        var systemPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.Id) && x.IsGloballyActive == true
        );
        if (systemPaymentMethod == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy phương thức thanh toán"));
        }

        return new GetSystemPaymentMethodByIdResponse()
        {
            Id = systemPaymentMethod.Id.ToString(),
            Name = systemPaymentMethod.Name,
            Code = systemPaymentMethod.Code,
            Description = systemPaymentMethod.Description ?? String.Empty,
            LogoUrl = systemPaymentMethod.LogoUrl ?? String.Empty,
            PaymentMethod = (PaymentMethod)systemPaymentMethod.Type,
        };
    }
}