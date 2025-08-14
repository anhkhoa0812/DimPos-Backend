using Confluent.Kafka;
using DimPos.Payment.Application.Common.Utils;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Models.PayOs;
using DimPos.Payment.Domain.Settings;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using DimPos.Payment.Infrastructure.Utils;
using DimPos.Store.Application.Common.Protos;
using MassTransit;
using Microsoft.Extensions.Options;
using Net.Codecrete.QrCodeGenerator;
using Net.payOS;
using Net.payOS.Types;
using SharedProject.Events.Payment.UpdatePaymentTransaction;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DimPos.Payment.Application.Services.Implement;

public class PayOsService : IPayOsService
{
    private readonly QrSettings _qrSettings;
    private readonly ILogger _logger;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ITopicProducer<Null, CallbackPaymentResponseModel> _topicProducer;
    public PayOsService(IOptions<QrSettings> qrSettings, ILogger logger,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        IUnitOfWork<PaymentContext> unitOfWork,
        ITopicProducer<Null, CallbackPaymentResponseModel> topicProducer)
    
    {
        _qrSettings = qrSettings.Value ?? throw new ArgumentNullException(nameof(qrSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    public async Task<string> CreateQr(CreateQrPayOsPaymentRequest request)
    {
        var payOsModel = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var payOs = new PayOS(payOsModel.ClientId, payOsModel.ApiKey, payOsModel.ChecksumKey);
        string shortId = Convert.ToBase64String(request.OrderId.ToByteArray())
            .Replace("/", "_") // tránh ký tự đặc biệt
            .Replace("+", "-")
            .TrimEnd('=');
        var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        var paymentData = new PaymentData(
            orderCode,
            (int) request.Amount,
            shortId,
            new List<ItemData>(),
            "",
            "",
            expiredAt: ((DateTimeOffset)TimeUtil.GetCurrentSEATime().AddMinutes(10)).ToUnixTimeSeconds()
        );
        var createPayment = await payOs.createPaymentLink(paymentData);
        if (createPayment.qrCode != null)
        {
            var qr = QrCode.EncodeText(createPayment.qrCode, QrCode.Ecc.Medium);
            // string svg = qr.ToSvgString(4);
            byte[] image = qr.ToPng(10, 4);
            var tempFile = Path.Combine(Path.GetTempPath(), $"{request.OrderId}.png");
            await File.WriteAllBytesAsync(tempFile, image);

            var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
            Directory.CreateDirectory(publicFolder);

            var fileName = Path.GetFileName(tempFile);
            var publicPath = Path.Combine(publicFolder, fileName);
            File.Copy(tempFile, publicPath, true);
        
            var publicUrl = $"{_qrSettings.QrLink}/{fileName}";
            return publicUrl;
        }
        throw new Exception("Lỗi khi tạo QR Code thanh toán với PayOS. Vui lòng thử lại sau.");
    }

    public async Task HandlePayOsCallback(WebhookType request)
    {
        _logger.Information("BEGIN: {HandlePayOsCallbackName} - {CurrentSeaTime}", nameof(HandlePayOsCallback), TimeUtil.GetCurrentSEATime());
        _logger.Information("Received PayOS callback with data: {RequestData}", request);
        var payOs = new PayOS("", "", "");
        var data = payOs.verifyPaymentWebhookData(request);
        
        Guid original = new Guid(Convert.FromBase64String(
            data.description.Replace("_", "/").Replace("-", "+") + "=="
        ));
        if (request.code.Equals("00"))
        {
            var callbackPaymentResponseModel = new CallbackPaymentResponseModel()
            {
                CorrelationId = Guid.CreateVersion7(),
                OrderId = original,
                TransCode = String.Empty,
                TransAmount = request.data.amount,
                TransStatus = MPosTransStatus.Settled,
                Type = PaymentCallbackType.MPos
            };
            await _topicProducer.Produce(
                null,
                callbackPaymentResponseModel
            ).ConfigureAwait(false);
        }
    }

    public async Task ConfirmWebhookUrl(ConfirmWebhookUrlRequest request)
    {
        try
        {
            var key = request.StoreId.ToString("N");
            var payOsModel = DecodeCredentialsConfig(request.ConfigCredential, key);
            var payOs = new PayOS(payOsModel.ClientId, payOsModel.ApiKey, payOsModel.ChecksumKey);

            await payOs.confirmWebhook(request.WebhookUrl);
            _logger.Information("Webhook URL confirmed for StoreId: {StoreId}", request.StoreId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    private PayOsModel DecodeCredentialsConfig(string data, string key)
    {
        var decodedData = CryptographyUtil.DecodeCredentialsConfig(data, key);
        var payOsModel = JsonSerializer.Deserialize<PayOsModel>(decodedData);

        return new PayOsModel()
        {
            ApiKey = payOsModel.ApiKey,
            ChecksumKey = payOsModel.ChecksumKey,
            ClientId = payOsModel.ClientId
        };
    }
}