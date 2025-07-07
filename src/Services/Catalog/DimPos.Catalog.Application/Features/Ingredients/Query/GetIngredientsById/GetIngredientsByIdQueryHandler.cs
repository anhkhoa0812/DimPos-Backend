using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Ingredients;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Query.GetIngredientsById;

public class GetIngredientsByIdQueryHandler : IRequestHandler<GetIngredientsByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetIngredientsByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetIngredientsByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");
        
        var ingredient = await _unitOfWork.GetRepository<Domain.Entities.Ingredients>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.IngredientId && x.BrandId == brandId
        );
        if (ingredient == null)
            throw new BadHttpRequestException("Thành phần không tồn tại hoặc không thuộc thương hiệu của bạn");
        var response = new GetIngredientsByIdResponse()
        {
            Id = ingredient.Id,
            Code = ingredient.Code,
            Sku = ingredient.Sku,
            Name = ingredient.Name,
            MeasureUnit = ingredient.MeasureUnit,
            Description = ingredient.Description,
            IsActive = ingredient.IsActive,
            CreatedDate = ingredient.CreatedDate,
            LastModifiedDate = ingredient.LastModifiedDate,
        };
        _logger.Information("Lấy thông tin thành phần {IngredientName} thành công", ingredient.Name);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin thành phần thành công",
            Data = response
        };
    }
}