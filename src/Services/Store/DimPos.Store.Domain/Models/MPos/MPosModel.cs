namespace DimPos.Store.Domain.Models.MPos;

public class MPosModel
{
    public long MerchantId { get; set; }
    public string Data { get; set; } = string.Empty;
}
public class MPosModelRequest
{
    public long MerchantId { get; set; }
    public MPosSettingDetails Settings { get; set; } = new MPosSettingDetails();
}

public class MPosSettingDetails
{
    public string SecretKey { get; set; }
    public string Muid {get; set;}
    public string PosId {get; set;}
}