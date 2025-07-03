using DimPos.Payment.Domain.Models.MPos.CancelEDCPayment;
using DimPos.Payment.Domain.Models.MPos.CancelQrPayment;
using DimPos.Payment.Domain.Models.MPos.EDCPayment;
using DimPos.Payment.Domain.Models.MPos.GetEDCStatus;
using DimPos.Payment.Domain.Models.MPos.GetQrStatus;
using DimPos.Payment.Domain.Models.MPos.RefundEDCPayment;
using DimPos.Payment.Domain.Models.Payment;

namespace DimPos.Payment.Application.Services.Interface;

public interface IMPosService
{
    Task<string> CreateQr(CreateQrPaymentRequest request);
    Task<EDCPaymentResponseData> CreateEDCPayment(CreateEDCPaymentRequest request);

    Task<CancelEDCResponseData> CancelEDCPayment(CreateCancelEDCRequest request);
    Task<CancelQrResponseData> CancelQrPayment(CreateCancelQrRequest request);
    
    Task<GetQrStatusResponseData> GetQrStatus(GetQrStatusRequest request);
    
    Task<GetEDCStatusResponseData> GetEDCStatus(GetEDCStatusRequest request);
    Task<RefundEDCPaymentResponseData> GetRefundEDCPayment(GetRefundEDCPaymentRequest request);
} 