using DimPos.Media.Application.Common.Protos;
using DimPos.Media.Application.Service.Implement;
using DimPos.Media.Application.Service.Interface;
using Grpc.Core;

namespace DimPos.Media.Application.GrpcService;

public class MediaGrpcService : Common.Protos.MediaGrpcService.MediaGrpcServiceBase
{
    private readonly IUploadService _uploadService;
    public MediaGrpcService(IUploadService uploadService)
    {
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
    }
    public override async Task<UploadImageResponse> UploadImage(IAsyncStreamReader<UploadImageRequest> requestStream, ServerCallContext context)
    {
        var imageResponses = new List<ImageResponse>();
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await Parallel.ForEachAsync(request.ListImageRequest.ImageRequest, async (imageRequest, ct) =>
            {
                var imageResponse = new ImageResponse()
                {
                    Id = imageRequest.Id,
                };
                
                var url = await _uploadService.UploadImageAsync(imageRequest.ChunkData);
                if (!string.IsNullOrEmpty(url))
                    imageResponse.ImageUrl = url;
                imageResponses.Add(imageResponse);
            });
            // _backgroundTaskQueue.QueueBackgroundWorkItem(new UploadImageJob()
            // {
            //     ImageUploadRequest = uploadImageRequest,
            //     OriginalUrls = imageResponses.Select(x => x.ImageUrl).ToList()
            // });
        }
        
        return new UploadImageResponse
        {
            ListImageResponse = new ListImageResponse()
            {
                ImageResponse =
                {
                    imageResponses
                }
            }
        };
    }
}