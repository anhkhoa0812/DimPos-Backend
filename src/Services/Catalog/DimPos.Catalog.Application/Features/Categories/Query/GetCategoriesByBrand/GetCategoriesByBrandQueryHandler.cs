using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Categories;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Categories.Query.GetCategoriesByBrand;

public class GetCategoriesByBrandQueryHandler : IRequestHandler<GetCategoriesByBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetCategoriesByBrandQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetCategoriesByBrandQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");
        }

        var categories = await _unitOfWork.GetRepository<Domain.Entities.Categories>().GetPagingListAsync(
            selector: x => new CategoriesResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                Type = x.Type,
                DisplayOrder = x.DisplayOrder,
                PictureUrl = x.PictureUrl,
                HasChildCategory = x.HasChildCategory,
                Status = x.Status
            },
            predicate: x => x.BrandId == brandId 
            && (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy danh sách danh mục thành công",
            Data = categories
        };

    }
}