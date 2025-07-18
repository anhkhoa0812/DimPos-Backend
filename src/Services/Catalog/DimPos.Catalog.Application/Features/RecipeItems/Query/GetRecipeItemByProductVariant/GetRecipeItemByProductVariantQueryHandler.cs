using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.RecipeItems;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.RecipeItems.Query.GetRecipeItemByProductVariant;

public class GetRecipeItemByProductVariantQueryHandler : IRequestHandler<GetRecipeItemByProductVariantQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetRecipeItemByProductVariantQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetRecipeItemByProductVariantQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");
        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId 
                            && x.Product.BrandId == brandId,
            include: x => x.Include(x => x.Product)
        );
        if (productVariant == null)
            throw new BadHttpRequestException("Biến thể sản phẩm không tồn tại hoặc không thuộc thương hiệu của bạn");
        var recipeItems = await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().GetPagingListAsync(
            selector: x => new GetRecipeItemByProductVariantResponse()
            {
                Id = x.Id,
                Quantity = x.Quantity,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate,
                CreatedByAccountId = x.CreatedByAccountId,
                Ingredient = new IngredientWithRecipeItemByProductVariantResponse()
                {
                    Id = x.Ingredient.Id,
                    Code = x.Ingredient.Code,
                    Sku = x.Ingredient.Sku,
                    Name = x.Ingredient.Name,
                    MeasureUnit = x.Ingredient.MeasureUnit,
                    Description = x.Ingredient.Description,
                    IsActive = x.Ingredient.IsActive,
                    CreatedDate = x.Ingredient.CreatedDate,
                    LastModifiedDate = x.Ingredient.LastModifiedDate
                }
            },
            predicate: x => x.ProductVariantId == request.ProductVariantId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thành phần theo biến thể sản phẩm thành công",
            Data = recipeItems
        };
    }
}