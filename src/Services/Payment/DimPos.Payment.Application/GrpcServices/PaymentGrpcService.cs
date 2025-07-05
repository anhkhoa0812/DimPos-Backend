using DimPos.Payment.Application.Common.Protos;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Domain.Models.Payment;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

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

    public override async Task<CreatePaymentTransactionResponse> CreatePaymentTransaction(CreatePaymentTransactionRequest request, ServerCallContext context)
    {
        var systemPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.SystemPaymentMethodId) && x.IsGloballyActive == true
        );
        if (systemPaymentMethod == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy phương thức thanh toán"));
        }

        var paymentTransaction = new PaymentTransactions()
        {
            Id = Guid.CreateVersion7(),
            StoreId = Guid.Parse(request.StoreId),
            BrandId = Guid.Parse(request.BrandId),
            Status = EPaymentTransactionStatus.PENDING,
            CustomerId = request.CustomerId != String.Empty ? Guid.Parse(request.CustomerId) : Guid.Empty,
            Amount = (decimal)request.Amount,
            OrderId = Guid.Parse(request.OrderId),
            SystemPaymentMethodTypeId = systemPaymentMethod.Id,
            TransactionType = ETransactionType.SALE_CAPTURE_B2C,
            CurrencyCode = "VND",
            ProcessedByAccountId = Guid.Parse(request.AccountId)
        };
        var qrLink = String.Empty;
        switch (systemPaymentMethod.Type)
        {
            case ESystemPaymentMethod.QR_EDC:
                await _mPosService.CreateEDCPayment(new CreateEDCPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = paymentTransaction.Amount,
                    Description = paymentTransaction.Description,
                    CredentialsConfig = request.CredentialsConfig,
                    PaymentMethod = EEDCPaymentMethod.QR,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            case ESystemPaymentMethod.CARD_EDC:
                await _mPosService.CreateEDCPayment(new CreateEDCPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = paymentTransaction.Amount,
                    Description = paymentTransaction.Description,
                    PaymentMethod = EEDCPaymentMethod.CARD,
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            case ESystemPaymentMethod.CASH:
                break;
            case ESystemPaymentMethod.QR_VIETQR:
                qrLink = await _mPosService.CreateQr(new CreateQrPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = (decimal)request.Amount,
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            default:
                throw new RpcException(new Status(StatusCode.Unimplemented, "Không hỗ trợ phương thức thanh toán này"));
        }
        await _unitOfWork.GetRepository<PaymentTransactions>().InsertAsync(paymentTransaction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Không thể tạo giao dịch thanh toán"));
        }
        return new CreatePaymentTransactionResponse()
        {
            QrLink = qrLink,
            SystemPaymentMethodName = systemPaymentMethod.Name
        };
    }

    public override async Task<GetSystemPaymentMethodListByIdResponse> GetSystemPaymentMethodListById(GetSystemPaymentMethodListByIdRequest request, ServerCallContext context)
    {
        var systemPaymentMethodId = request.SystemPaymentMethodIds.Select(Guid.Parse).ToList();
        
        var systemPaymentMethods = await _unitOfWork.GetRepository<SystemPaymentMethods>().GetListAsync(
            predicate: x => systemPaymentMethodId.Contains(x.Id) && x.IsGloballyActive == true
        );
        if(systemPaymentMethods.Count != systemPaymentMethodId.Count)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy một hoặc nhiều phương thức thanh toán"));
        }
        var response = new GetSystemPaymentMethodListByIdResponse();
        foreach (var systemPaymentMethod in systemPaymentMethods)
        {
            response.SystemPaymentMethods.Add(new GetSystemPaymentMethodByIdResponse()
            {
                Id = systemPaymentMethod.Id.ToString(),
                Name = systemPaymentMethod.Name,
                Code = systemPaymentMethod.Code,
                Description = systemPaymentMethod.Description ?? String.Empty,
                LogoUrl = systemPaymentMethod.LogoUrl ?? String.Empty,
                PaymentMethod = (PaymentMethod) systemPaymentMethod.Type
            });
        }
        return response;
    }

    public override async Task<UpdatePaymentMethodForOrderResponse> UpdatePaymentMethodForOrder(UpdatePaymentMethodForOrderRequest request, ServerCallContext context)
    {
        var paymentTransaction = await _unitOfWork.GetRepository<PaymentTransactions>().SingleOrDefaultAsync(
            predicate: x => x.StoreId == Guid.Parse(request.StoreId) &&
                            x.OrderId == Guid.Parse(request.OrderId)
        );
        if (paymentTransaction == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy giao dịch thanh toán cho đơn hàng này"));
        }

        if (paymentTransaction.Status != EPaymentTransactionStatus.PENDING)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Chỉ có thể cập nhật phương thức thanh toán cho giao dịch đang chờ xử lý"));
        }

        if (paymentTransaction.SystemPaymentMethodTypeId == Guid.Parse(request.SystemPaymentMethodId))
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Phương thức thanh toán đã được cập nhật trước đó"));
        }
        var existingPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == paymentTransaction.SystemPaymentMethodTypeId && x.IsGloballyActive == true
        );
        var systemPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == Guid.Parse(request.SystemPaymentMethodId) && x.IsGloballyActive == true
        );
        if (systemPaymentMethod == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy phương thức thanh toán"));
        }
        
        var qrLink = String.Empty;
        switch (existingPaymentMethod.Type)
        {
            case ESystemPaymentMethod.CARD_EDC:
                await _mPosService.CancelEDCPayment(new CreateCancelEDCRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N"),
                    Amount = (decimal)request.Amount,
                });
                break;
            case ESystemPaymentMethod.QR_EDC:
                await _mPosService.CancelEDCPayment(new CreateCancelEDCRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N"),
                    Amount = (decimal)request.Amount,
                });
                break;
            case ESystemPaymentMethod.QR_VIETQR:
                await _mPosService.CancelQrPayment(new CreateCancelQrRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N"),
                    Amount = (decimal)request.Amount,
                });
                break;
            case ESystemPaymentMethod.CASH:
                break;
            default:
                throw new RpcException(new Status(StatusCode.Unimplemented, "Không hỗ trợ phương thức thanh toán này"));
        }
        switch (systemPaymentMethod.Type)
        {
            case ESystemPaymentMethod.QR_EDC:
                await _mPosService.CreateEDCPayment(new CreateEDCPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = (decimal) request.Amount,
                    Description = paymentTransaction.Description ?? String.Empty,
                    CredentialsConfig = request.CredentialsConfig,
                    PaymentMethod = EEDCPaymentMethod.QR,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            case ESystemPaymentMethod.CARD_EDC:
                await _mPosService.CreateEDCPayment(new CreateEDCPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = paymentTransaction.Amount,
                    Description = paymentTransaction.Description,
                    PaymentMethod = EEDCPaymentMethod.CARD,
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            case ESystemPaymentMethod.CASH:
                break;
            case ESystemPaymentMethod.QR_VIETQR:
                qrLink = await _mPosService.CreateQr(new CreateQrPaymentRequest()
                {
                    OrderId = Guid.Parse(request.OrderId),
                    Amount = (decimal) request.Amount,
                    CredentialsConfig = request.CredentialsConfig,
                    Key = Guid.Parse(request.StoreId).ToString("N")
                });
                break;
            default:
                throw new RpcException(new Status(StatusCode.Unimplemented, "Không hỗ trợ phương thức thanh toán này"));
        }
        
        paymentTransaction.SystemPaymentMethodTypeId = Guid.Parse(request.SystemPaymentMethodId);
        _unitOfWork.GetRepository<PaymentTransactions>().UpdateAsync(paymentTransaction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Không thể cập nhật phương thức thanh toán cho giao dịch"));
        }
        
        return new UpdatePaymentMethodForOrderResponse()
        {
            QrLink = qrLink,
            SystemPaymentMethodName = systemPaymentMethod.Name,
        };
    }
}