using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.GetEDCStatus;

public class GetEDCStatusResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("transStatus")]
    public int TransStatus { get; set; }
    [JsonPropertyName("issuerCode")]
    public string IssuerCode { get; set; } // Loại thẻ hoặc loại QR
    [JsonPropertyName("pan")]
    public string? Pan { get; set; } // 6 số đầu và 4 số cuối của thẻ hoặc số tài khoản (VAQR)
    [JsonPropertyName("cardToken")]
    public string? CardToken { get; set; } 
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; } // Tên khách hàng
    [JsonPropertyName("expDate")]
    public string? ExpDate { get; set; } // Số điện thoại khách hàng
    [JsonPropertyName("authCode")]
    public string? AuthCode { get; set; } // Mã chuẩn chỉ (Giao dịch thẻ)
    [JsonPropertyName("rrn")]
    public string? Rrn { get; set; } // Mã tham chiếu trên hệ thống ngân hàng
    [JsonPropertyName("transCode")]
    public string? TransCode { get; set; } // Mã giao dịch trên hệ thống MPOS
    [JsonPropertyName("paymentIdentifier")]
    public string? PaymentIdentifier { get; set; } // Mã thanh toán trên EDC
    [JsonPropertyName("transDate")]
    public long? TransDate { get; set; }
    [JsonPropertyName("refundHistory")]
    public string? RefundHistory { get; set; } //Chuỗi JSON chứa lịch sử hoàn tiền, nếu có
    [JsonPropertyName("paymentType")]
    public string? PaymentType { get; set; } // NORMAL, INSTALLMENT, MOTO, DEPOSIT
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; } // CARD, QR, LINK
    [JsonPropertyName("qrType")]
    public string? QrType { get; set; }
    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }
    [JsonPropertyName("period")]
    public int? Period { get; set; }
    [JsonPropertyName("customerMobile")]
    public string? CustomerMobile { get; set; }
    [JsonPropertyName("customerID")]
    public string? CustomerId { get; set; }
    [JsonPropertyName("customerEmail")]
    public string? CustomerEmail { get; set; }
    
}