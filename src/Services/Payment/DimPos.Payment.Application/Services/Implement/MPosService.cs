using System.Text;
using System.Text.Json;
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
using DimPos.Payment.Domain.Models.MPos.RefundEDCPayment;
using DimPos.Payment.Domain.Models.Payment;
using DimPos.Payment.Domain.Settings;
using Microsoft.Extensions.Options;
using Net.Codecrete.QrCodeGenerator;

namespace DimPos.Payment.Application.Services.Implement;

public class MPosService : IMPosService
{
    private readonly HttpClient _httpClient;
    private MPosSettings _settings;
    private readonly string _qrUrl;
    private readonly string _edcPaymentUrl;
    private readonly string _refundEdcUrl;
    public MPosService(HttpClient httpClient, IOptions<MPosSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _qrUrl = _settings.DevDomain + "/orderQR";
        _edcPaymentUrl = _settings.DevDomain + "/order";
        _refundEdcUrl = _settings.DevDomain + "/transaction";
    }
    public async Task<string> CreateQr(CreateQrPaymentRequest request)
    {
        var createQrRequestData = new CreateQrRequestData()
        {
            ServiceName = nameof(EServiceName.CREATE_QR),
            OrderId = Guid.CreateVersion7().ToString(),
            Amount = request.Amount.ToString(),
            Description = "Mã QR thanh toán đơn haàng",
            Muid = _settings.Muid,
            QrType = nameof(EQrType.VAQR)
        };
        
        var json = EncodeData(createQrRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CreateQrResponseData>(mPosResponse.ResData);
        
        var qr = QrCode.EncodeText(responseData.QrCode, QrCode.Ecc.Medium);
        string svg = qr.ToSvgString(4);
        
        var tempFile = Path.Combine(Path.GetTempPath(), $"{responseData.OrderId}.svg");
        await File.WriteAllTextAsync(tempFile, svg, Encoding.UTF8);

        var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
        Directory.CreateDirectory(publicFolder);

        var fileName = Path.GetFileName(tempFile);
        var publicPath = Path.Combine(publicFolder, fileName);
        File.Copy(tempFile, publicPath, true);
        
        var publicUrl = $"https://localhost:7276/temp/{fileName}";
        return publicUrl;
    }

    public async Task<EDCPaymentResponseData> CreateEDCPayment(CreateEDCPaymentRequest request)
    {
        var createEDCPaymentRequestData = new EDCPaymentRequestData()
        {
            ServiceName = nameof(EServiceName.ADD_ORDER_INFOR),
            OrderId = Guid.CreateVersion7().ToString(),
            Amount = request.Amount.ToString(),
            Description = "Thanh toán đơn hàng qua EDC",
            PosId = _settings.PosId,
            PaymentType = null,
            PaymentMethod = request.PaymentMethod.ToString()
        };
        var json = EncodeData(createEDCPaymentRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<EDCPaymentResponseData>(mPosResponse.ResData);
        return responseData;
    }

    public async Task<CancelEDCResponseData> CancelEDCPayment(CreateCancelEDCRequest request)
    {
        var cancelEDCPaymentRequestData = new CancelEDCRequestData()
        {
            ServiceName = nameof(EServiceName.REMOVE_ORDER_INFOR),
            OrderId = request.OrderId.ToString(),
            Amount = request.Amount.ToString(),
            PosId = _settings.PosId,
        };
        
        var json = EncodeData(cancelEDCPaymentRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CancelEDCResponseData>(mPosResponse.ResData);
        return responseData;
    }

    public async Task<CancelQrResponseData> CancelQrPayment(CreateCancelQrRequest request)
    {
        var cancelQrRequestData = new CancelQrRequestData()
        {
            ServiceName = nameof(EServiceName.REMOVE_QR),
            OrderId = request.OrderId.ToString(),
            Muid = _settings.Muid,
            Amount = request.Amount.ToString(),
            QrType = nameof(EQrType.VAQR)
        };
        var json = EncodeData(cancelQrRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<CancelQrResponseData>(mPosResponse.ResData);
        return responseData;
    }

    public async Task<GetQrStatusResponseData> GetQrStatus(Guid orderId)
    {
        var getQrStatusRequestData = new GetQrStatusRequestData()
        {
            ServiceName = nameof(EServiceName.QR_GET_TRANSACTION_STATUS),
            OrderId = orderId.ToString(),
            Muid = _settings.Muid,
            Amount = "0", // Amount không cần thiết trong trường hợp này, nhưng vẫn cần truyền vào để phù hợp với y/cầu
        };
        
        var json = EncodeData(getQrStatusRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_qrUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<GetQrStatusResponseData>(mPosResponse.ResData);
        return responseData;
    }

    public async Task<GetEDCStatusResponseData> GetEDCStatus(Guid orderId)
    {
        var getEDCStatusRequestData = new GetEDCStatusRequestData()
        {
            ServiceName = nameof(EServiceName.GET_TRANSACTION_STATUS),
            PosId = _settings.PosId,
            OrderId = orderId.ToString()
        };
        var json = EncodeData(getEDCStatusRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_edcPaymentUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Failed to deserialize MPosResponse");
        }
        var responseData = DecodeData<GetEDCStatusResponseData>(mPosResponse.ResData);
        return responseData;
    }

    public async Task<RefundEDCPaymentResponseData> GetRefundEDCPayment(Guid orderId)
    {
        var getEDCStatusResponseData = await GetEDCStatus(orderId);
        if (getEDCStatusResponseData == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giao dịch của đơn hàng này");
        }

        if (getEDCStatusResponseData.TransStatus != 104) 
            throw new BadHttpRequestException("Trạng thái giao dịch không hợp lệ, không thể hoàn tiền");

        var refundEDCPaymentRequestData = new RefundEDCPaymentRequestData()
        {
            ServiceName = nameof(EServiceName.REFUND_TRANSACTION),
            OrderId = orderId.ToString(),
            PosId = _settings.PosId,
            TransCode = getEDCStatusResponseData.TransCode,
            RefundAmount = getEDCStatusResponseData.Amount
        };
        var json = EncodeData(refundEDCPaymentRequestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_refundEdcUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        var mPosResponse = JsonSerializer.Deserialize<MPosResponse>(responseString);
        
        if (mPosResponse != null && mPosResponse.ResCode != 200)
        {
            throw new Exception("Lỗi khi thực hiện hoàn tiền: " + mPosResponse.Message);
        }
        
        var responseData = DecodeData<RefundEDCPaymentResponseData>(mPosResponse.ResData);
        return responseData;

    }

    private string EncodeData<T>(T data)
    {
        var dataSerialized = JsonSerializer.Serialize(data);
        var mPosRequest = new MPosRequest()
        {
            MerchantId = _settings.MerchantId,
            ReqData = CryptographyUtil.Encode(dataSerialized, _settings.SecretKey)
        };
        return JsonSerializer.Serialize(mPosRequest);
    }
    private T DecodeData<T>(string data)
    {
        var decodedData = CryptographyUtil.Decode(data, _settings.SecretKey);
        T decodedResponseData = JsonSerializer.Deserialize<T>(decodedData) ?? throw new ArgumentException("decode data failed");
        return decodedResponseData;
    }
}