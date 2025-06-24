using System.Net;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Categories;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Query.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetCategoryByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var category = await _unitOfWork.GetRepository<Domain.Entities.Categories>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.CategoryId && x.BrandId == brandId,
            selector: x => new GetCategoryByIdResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                PictureUrl = x.PictureUrl,
                Description = x.Description,
                Type = x.Type,
                DisplayOrder = x.DisplayOrder,
                HasChildCategory = x.HasChildCategory,
                Status = x.Status,
                ParentCategory = x.Parent != null ? new ParentCategoryResponse()
                {
                    Id = x.Parent.Id,
                    Code = x.Parent.Code,
                    Name = x.Parent.Name,
                    Description = x.Parent.Description,
                    Type = x.Parent.Type,
                    DisplayOrder = x.Parent.DisplayOrder,
                    PictureUrl = x.Parent.PictureUrl,
                    HasChildCategory = x.Parent.HasChildCategory,
                    Status = x.Parent.Status
                } : null
            }
        );
        if (category == null)
            throw new BadHttpRequestException("Không tìm thấy danh mục");
        return new ApiResponse()
        {
            Status = (int)HttpStatusCode.OK,
            Message = "Lấy dữ liệu thành công",
            Data = category
        };
    }
}