using DimPos.Payment.Domain.Models.PayOs;
using Net.payOS.Types;

namespace DimPos.Payment.Application.Services.Interface;

public interface IPayOsService
{
    Task<string> CreateQr(CreateQrPayOsPaymentRequest request);
    Task HandlePayOsCallback(WebhookType request);
    Task ConfirmWebhookUrl(ConfirmWebhookUrlRequest request);
}