using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Ingredients;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Query.GetIngredientsByBrand;

public class GetIngredientsByBrandQueryHandler : IRequestHandler<GetIngredientsByBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetIngredientsByBrandQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetIngredientsByBrandQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("BEGIN: GetIngredientsByBrandQueryHandler.Handle");
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");

        var ingredients = await _unitOfWork.GetRepository<Domain.Entities.Ingredients>().GetPagingListAsync(
            selector: x => new GetIngredientsByBrandResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Sku = x.Sku,
                Name = x.Name,
                MeasureUnit = x.MeasureUnit,
                Description = x.Description,
                IsActive = x.IsActive
            },
            predicate: x => x.BrandId == brandId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        _logger.Information("END: GetIngredientsByBrandQueryHandler.Handle");
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thành phần thành công",
            Data = ingredients
        };
    }
}