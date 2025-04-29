namespace DimPos.Catalog.Application.Services.Interface;

public interface IUploadService
{
    Task<string> UploadImageAsync(IFormFile file);
}