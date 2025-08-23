using System.Threading.Channels;
using DimPos.Media.Application.Common.Models.Settings;
using DimPos.Media.Application.Service.Interface;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace DimPos.Media.Application.Service.Implement;

public class UploadService : IUploadService
{
    private readonly S3CompatibleStorageSettings _s3CompatibleStorageSettings;
    public UploadService(IOptions<S3CompatibleStorageSettings> s3CompatibleStorageSettings)
    {
        _s3CompatibleStorageSettings = s3CompatibleStorageSettings.Value;
    }

    public async Task<string> UploadImageAsync(ByteString byteString)
    {
        var fileName = $"{Guid.NewGuid()}.jpg";
        // var allowedExtensions = new[] { ".jpeg", ".png", ".jpg", ".gif", ".bmp", ".webp" };
        // var extension = Path.GetExtension(file.FileName).ToLower();
        // if (!allowedExtensions.Contains(extension))
        //     throw new InvalidOperationException(
        //         "Chỉ các định dạng tệp txt, .jpeg, .png, .jpg, .gif, .bmp, và .webp được phép tải lên.");
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
            // var objectName = $"{Guid.CreateVersion7().ToString()}{extension}";
            using var stream = new MemoryStream(byteString.ToByteArray());            
            var result = await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_s3CompatibleStorageSettings.BucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("image/jpeg")
                .WithHeaders(headers)
            );
            if (result == null)
                throw new MinioException("Failed to upload image");
            return $"https://{_s3CompatibleStorageSettings.EndPoint}/{_s3CompatibleStorageSettings.BucketName}/{fileName}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> UploadExcelAsync(ByteString byteString)
    {
        var fileName = $"{Guid.NewGuid()}.xlsx";
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
            // var objectName = $"{Guid.CreateVersion7().ToString()}{extension}";
            using var stream = new MemoryStream(byteString.ToByteArray());            
            var result = await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_s3CompatibleStorageSettings.BucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/vnd.ms-excel")
                .WithHeaders(headers)
            );
            if (result == null)
                throw new MinioException("Failed to upload image");
            return $"https://{_s3CompatibleStorageSettings.EndPoint}/{_s3CompatibleStorageSettings.BucketName}/{fileName}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}