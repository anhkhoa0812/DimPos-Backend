using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Settings;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace DimPos.Catalog.Application.Services.Implement;

public class UploadService : IUploadService
{
    private readonly ILogger _logger;
    private readonly S3CompatibleStorageSettings _s3CompatibleStorageSettings;

    public UploadService(ILogger logger, IOptions<S3CompatibleStorageSettings> s3CompatibleStorageSettings)
    {
        _logger = logger;
        _s3CompatibleStorageSettings = s3CompatibleStorageSettings.Value;
    }
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpeg", ".png", ".jpg", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                "Chỉ các định dạng tệp txt, .jpeg, .png, .jpg, .gif, .bmp, và .webp được phép tải lên.");
        try
        {
            var minio = new MinioClient()
                .WithEndpoint(_s3CompatibleStorageSettings.EndPoint)
                .WithCredentials(_s3CompatibleStorageSettings.AccessKey, _s3CompatibleStorageSettings.SecretKey)
                .Build();
            var headers = new Dictionary<string, string>
            {
                { "x-amz-acl", "public-read" }
            };
            var objectName = $"{Guid.NewGuid().ToString()}{extension}";
            var result = await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_s3CompatibleStorageSettings.BucketName)
                .WithObject(objectName)
                .WithStreamData(file.OpenReadStream())
                .WithObjectSize(file.Length)
                .WithContentType("image/jpeg")
                .WithHeaders(headers)
            );
            if (result == null)
                throw new MinioException("Failed to upload image");
            return $"https://{_s3CompatibleStorageSettings.EndPoint}/{_s3CompatibleStorageSettings.BucketName}/{objectName}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}