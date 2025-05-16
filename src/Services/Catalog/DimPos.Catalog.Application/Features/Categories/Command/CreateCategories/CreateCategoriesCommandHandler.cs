using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;

public class CreateCategoriesCommandHandler : IRequestHandler<CreateCategoriesCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IUploadService _uploadService;
    private readonly IClaimService _claimService;
    public CreateCategoriesCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IUploadService uploadService, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateCategoriesCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        _logger.Information($"BEGIN: {nameof(CreateCategoriesCommandHandler)} - {DateTime.UtcNow}");
        var category = CategoriesMapper.ToCategories(request);
        category.Id = Guid.CreateVersion7();
        category.HasChildCategory = false;
        category.BrandId = brandId;
        if (request.ParentId != null)
        {
            var parentCategory = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
                predicate: c => c.Id == request.ParentId.Value
            );
            if (parentCategory != null)
            {
                category.ParentId = parentCategory.Id;
                parentCategory.HasChildCategory = true;
                _unitOfWork.GetRepository<Domain.Entities.Categories>().UpdateAsync(parentCategory);
            }
            else
            {
                return new ApiResponse()
                {
                    Message = "Không tìm thấy danh mục cha",
                    Status = (int) HttpStatusCode.NotFound,
                };
            }
        }

        if (request.Image != null)
        {
            var imageUrl = await _uploadService.UploadImageAsync(request.Image);
            if(imageUrl == null)
            {
                return new ApiResponse()
                {
                    Message = "Lỗi khi tải ảnh lên",
                    Status =  (int) HttpStatusCode.InternalServerError,
                };
            }
            category.PictureUrl = imageUrl;
        }
        await _unitOfWork.GetRepository<Domain.Entities.Categories>().InsertAsync(category); 
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateCategoriesCommandHandler)} - {DateTime.UtcNow}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = (int) HttpStatusCode.Created,
                Message = "Tạo danh mục thành công",
            };
        }
        return new ApiResponse()
        {
            Status = (int) HttpStatusCode.InternalServerError,
            Message = "Tạo danh mục thất bại",
        };
    }
}