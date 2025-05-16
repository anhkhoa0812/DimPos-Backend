using Google.Protobuf;

namespace DimPos.Media.Application.Service.Interface;

public interface IUploadService
{
    Task<string> UploadImageAsync(ByteString byteString);
}