using System.Globalization;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using DimPos.Payment.Application.Common.Utils;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Domain.Models.MPos.Base;
using DimPos.Payment.Domain.Models.MPos.CancelEDCPayment;
using DimPos.Payment.Domain.Models.MPos.CancelQrPayment;
using DimPos.Payment.Domain.Models.MPos.CreateQr;
using DimPos.Payment.Domain.Models.MPos.EDCPayment;
using DimPos.Payment.Domain.Models.MPos.GetEDCStatus;
using DimPos.Payment.Domain.Models.MPos.GetQrStatus;
using DimPos.Payment.Domain.Models.MPos.MPosCallback;
using DimPos.Payment.Domain.Models.MPos.RefundEDCPayment;
using DimPos.Payment.Domain.Models.Payment;
using DimPos.Payment.Domain.Settings;
using DimPos.Store.Application.Common.Protos;
using MassTransit;
using Microsoft.Extensions.Options;
using Net.Codecrete.QrCodeGenerator;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Payment.Application.Services.Implement;

public class MPosService : IMPosService
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly MPosSettings _settings;
    private readonly QrSettings _qrSettings;
    private readonly string _qrUrl;
    private readonly string _edcPaymentUrl;
    private readonly string _refundEdcUrl;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly ITopicProducer<Null, CallbackPaymentResponseModel> _topicProducer;
    public MPosService(HttpClient httpClient, IOptions<MPosSettings> settings,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService, ILogger logger,
        ITopicProducer<Null, CallbackPaymentResponseModel> topicProducer,
        IOptions<QrSettings> qrSettings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _qrUrl = _settings.Domain + "/orderQR";
        _edcPaymentUrl = _settings.Domain + "/order";
        _refundEdcUrl = _settings.Domain + "/transaction";
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
        _qrSettings = qrSettings.Value ?? throw new ArgumentNullException(nameof(qrSettings));
    }
    public async Task<string> CreateQr(CreateQrPaymentRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var createQrRequestData = new CreateQrRequestData()
        {
            ServiceName = nameof(EServiceName.CREATE_QR),
            OrderId = request.OrderId.ToString(),
            Amount = request.Amount.ToString(),
            Description = "Mã QR thanh toán đơn hàng",
            Muid = decodeCredentialsConfig.Settings.Muid,
            QrType = nameof(EQrType.VAQR)
        };
        
        var json = EncodeData(createQrRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();
        _logger.Information("CreateQrResponse: {@ResponseString}", responseString);
        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CreateQrResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        
        _logger.Information("CreateQrResponseData: {@ResponseData}", responseData);
        
        var qr = QrCode.EncodeText(responseData.QrCode, QrCode.Ecc.Medium);
        // string svg = qr.ToSvgString(4);
        byte[] image = qr.ToPng(10, 4);
        var tempFile = Path.Combine(Path.GetTempPath(), $"{responseData.OrderId}.png");
        await File.WriteAllBytesAsync(tempFile, image);

        var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
        Directory.CreateDirectory(publicFolder);

        var fileName = Path.GetFileName(tempFile);
        var publicPath = Path.Combine(publicFolder, fileName);
        File.Copy(tempFile, publicPath, true);
        
        var publicUrl = $"{_qrSettings.QrLink}/{fileName}";
        return publicUrl;
    }

    public async Task<EDCPaymentResponseData> CreateEDCPayment(CreateEDCPaymentRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var createEDCPaymentRequestData = new EDCPaymentRequestData()
        {
            ServiceName = nameof(EServiceName.ADD_ORDER_INFOR),
            OrderId = request.OrderId.ToString(),
            Amount = request.Amount.ToString(),
            Description = "Thanh toán đơn hàng qua EDC",
            PosId = decodeCredentialsConfig.Settings.PosId,
            PaymentType = null,
            PaymentMethod = request.PaymentMethod.ToString()
        };
        var json = EncodeData(createEDCPaymentRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<EDCPaymentResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        _logger.Information("CreateEDCPayment: {@ResponseData}", responseData);
        return responseData;
    }

    public async Task<CancelEDCResponseData> CancelEDCPayment(CreateCancelEDCRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var cancelEDCPaymentRequestData = new CancelEDCRequestData()
        {
            ServiceName = nameof(EServiceName.REMOVE_ORDER_INFOR),
            OrderId = request.OrderId.ToString(),
            Amount = request.Amount.ToString(),
            PosId = decodeCredentialsConfig.Settings.PosId,
        };
        
        var json = EncodeData(cancelEDCPaymentRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CancelEDCResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        return responseData;
    }

    public async Task<CancelQrResponseData> CancelQrPayment(CreateCancelQrRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var cancelQrRequestData = new CancelQrRequestData()
        {
            ServiceName = nameof(EServiceName.REMOVE_QR),
            OrderId = request.OrderId.ToString(),
            Muid = decodeCredentialsConfig.Settings.Muid,
            Amount = request.Amount.ToString(),
            QrType = nameof(EQrType.VAQR)
        };
        var json = EncodeData(cancelQrRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CancelQrResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        return responseData;
    }

    public async Task<GetQrStatusResponseData> GetQrStatus(GetQrStatusRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var getQrStatusRequestData = new GetQrStatusRequestData()
        {
            ServiceName = nameof(EServiceName.QR_GET_TRANSACTION_STATUS),
            OrderId = request.OrderId.ToString(),
            Muid = decodeCredentialsConfig.Settings.Muid,
            Amount = "0", // Amount không cần thiết trong trường hợp này, nhưng vẫn cần truyền vào để phù hợp với y/cầu
        };
        
        var json = EncodeData(getQrStatusRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<GetQrStatusResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        return responseData;
    }

    public async Task<GetEDCStatusResponseData> GetEDCStatus(GetEDCStatusRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var getEDCStatusRequestData = new GetEDCStatusRequestData()
        {
            ServiceName = nameof(EServiceName.GET_TRANSACTION_STATUS),
            PosId = decodeCredentialsConfig.Settings.PosId,
            OrderId = request.OrderId.ToString()
        };
        var json = EncodeData(getEDCStatusRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<GetEDCStatusResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        return responseData;
    }

    public async Task<RefundEDCPaymentResponseData> GetRefundEDCPayment(GetRefundEDCPaymentRequest request)
    {
        var decodeCredentialsConfig = DecodeCredentialsConfig(request.CredentialsConfig, request.Key);
        var getEDCStatusResponseData = await GetEDCStatus(new GetEDCStatusRequest()
        {
            OrderId = request.OrderId,
            CredentialsConfig = request.CredentialsConfig,
            Key = request.Key
        });
        if (getEDCStatusResponseData == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giao dịch của đơn hàng này");
        }

        if (getEDCStatusResponseData.TransStatus != 104) 
            throw new BadHttpRequestException("Trạng thái giao dịch không hợp lệ, không thể hoàn tiền");

        var refundEDCPaymentRequestData = new RefundEDCPaymentRequestData()
        {
            ServiceName = nameof(EServiceName.REFUND_TRANSACTION),
            OrderId = request.OrderId.ToString(),
            PosId = decodeCredentialsConfig.Settings.PosId,
            TransCode = getEDCStatusResponseData.TransCode,
            RefundAmount = getEDCStatusResponseData.Amount
        };
        var json = EncodeData(refundEDCPaymentRequestData, decodeCredentialsConfig);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_refundEdcUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Lỗi khi thực hiện hoàn tiền: " + mPosResponse.Message);
        }
        
        var responseData = DecodeData<RefundEDCPaymentResponseData>(mPosResponse.ResData, decodeCredentialsConfig);
        return responseData;
    }

    public async Task HandleMPosCallback(MPosRequest request)
    {
        _logger.Information("Received MPos callback: {@Request}", request);
        var credentialsConfig = await _storeGrpcService.GetCredentialsConfigByMerchantIdAsync(
            new GetCredentialsConfigByMerchantIdRequest()
            {
                MerchantId = request.MerchantId
            });
        if (credentialsConfig == null || string.IsNullOrEmpty(credentialsConfig.CredentialsConfig))
        {
            return;
        }
        var mPosModelRequest = DecodeCredentialsConfig(credentialsConfig.CredentialsConfig, credentialsConfig.StoreId);
        var callbackRequestData = DecodeData<MPosCallbackRequest>(request.ReqData, mPosModelRequest );
        if (callbackRequestData == null)
        {
            throw new BadHttpRequestException("Không thể giải mã dữ liệu callback");
        }
        _logger.Information("Received MPos callback: {@CallbackRequestData}", callbackRequestData);
        var status = Enum.IsDefined(typeof(MPosTransStatus), (int) callbackRequestData.TransStatus)
            ? (MPosTransStatus?) (int) callbackRequestData.TransStatus : null;
        if (status != null && (status == MPosTransStatus.Settled ||
            status == MPosTransStatus.Fail ||
            status == MPosTransStatus.Rejected ||
            status == MPosTransStatus.Voided))
        {
            var callbackPaymentResponseModel = new CallbackPaymentResponseModel()
            {
                CorrelationId = Guid.CreateVersion7(),
                OrderId = Guid.Parse(callbackRequestData.OrderId),
                TransCode = callbackRequestData.TransCode,
                TransAmount = callbackRequestData.TransAmount,
                TransStatus = status.Value,
                Type = PaymentCallbackType.MPos
            };
            await _topicProducer.Produce(
                null,
                callbackPaymentResponseModel
            ).ConfigureAwait(false);
            _logger.Information("Published UpdatePaymentTransactionRequestModel: {@CallbackPaymentResponseModel}", callbackPaymentResponseModel);
        }
        
    }

    private string EncodeData<T>(T data, MPosModelRequest credentialsConfig)
    {
        var dataSerialized = JsonSerializer.Serialize(data);
        var mPosRequest = new MPosRequest()
        {
            MerchantId = credentialsConfig.MerchantId,
            ReqData = CryptographyUtil.Encode(dataSerialized, credentialsConfig.Settings.SecretKey)
        };
        return JsonSerializer.Serialize(mPosRequest);
    }
    private T DecodeData<T>(string data, MPosModelRequest credentialsConfig)
    {
        var decodedData = CryptographyUtil.Decode(data, credentialsConfig.Settings.SecretKey);
        T decodedResponseData = JsonSerializer.Deserialize<T>(decodedData) ?? throw new ArgumentException("decode data failed");
        return decodedResponseData;
    }

    private MPosModelRequest DecodeCredentialsConfig(string data, string key)
    {
        var mPosModel = JsonSerializer.Deserialize<MPosModel>(data);
        var decodedData = CryptographyUtil.DecodeCredentialsConfig(mPosModel.Data, key);
        var mPosModelRequestData = JsonSerializer.Deserialize<MPosSettingDetails>(decodedData);

        return new MPosModelRequest()
        {
            MerchantId = mPosModel.MerchantId,
            Settings = mPosModelRequestData
        };
    }
}