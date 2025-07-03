namespace DimPos.Payment.Domain.Models.MPos.Base;

public class MPosModel
{
    public long MerchantId { get; set; }
    public string SecretKey { get; set; }
    public string Muid {get; set;}
    public string PosId {get; set;}
}