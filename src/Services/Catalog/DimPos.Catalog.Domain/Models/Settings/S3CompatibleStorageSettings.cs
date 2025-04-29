namespace DimPos.Catalog.Domain.Models.Settings;

public class S3CompatibleStorageSettings
{
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string BucketName { get; set; }
    public string EndPoint { get; set; }
}