namespace DimPos.Payment.Domain.Settings;

public class MPosSettings
{
    public long MerchantId { get; set; }
    public string SecretKey { get; set; }
    public string Muid {get; set;}
    public string DevDomain { get; set; }
    public string Domain { get; set; }
    public string PosId {get; set;}
}