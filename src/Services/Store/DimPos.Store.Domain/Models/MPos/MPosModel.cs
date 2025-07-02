namespace DimPos.Store.Domain.Models.MPos;

public class MPosModel
{
    public long MerchantId { get; set; }
    public string SecretKey { get; set; }
    public string Muid {get; set;}
    public string PosId {get; set;}
}