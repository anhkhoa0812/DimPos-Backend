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
            predicate: x => x.Id == request.CategoryId && x.BrandId == brandId
        );
        if (category == null)
            throw new BadHttpRequestException("Không tìm thấy danh mục");
        var response = new GetCategoryByIdResponse()
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            PictureUrl = category.PictureUrl,
            Description = category.Description,
            Type = category.Type,
            DisplayOrder = category.DisplayOrder,
            HasChildCategory = category.HasChildCategory,
            Status = category.Status,
        };
        return new ApiResponse()
        {
            Status = (int)HttpStatusCode.OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }
}