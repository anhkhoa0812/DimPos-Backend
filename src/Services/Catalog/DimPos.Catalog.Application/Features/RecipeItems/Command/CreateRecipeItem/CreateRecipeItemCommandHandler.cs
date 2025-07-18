using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.CreateRecipeItem;

public class CreateRecipeItemCommandHandler : IRequestHandler<CreateRecipeItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public CreateRecipeItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateRecipeItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");

        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin người dùng");

        var productVariant = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductVariantId 
                            && x.Product.BrandId == brandId,
            include: x => x.Include(x => x.Product)
        );
        if (productVariant == null)
            throw new BadHttpRequestException("Biến thể sản phẩm không tồn tại hoặc không thuộc thương hiệu của bạn");
        var ingredient = await _unitOfWork.GetRepository<Domain.Entities.Ingredients>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.IngredientId 
                            && x.BrandId == brandId
                            && x.IsActive
        );
        if (ingredient == null)
            throw new BadHttpRequestException("Thành phần không tồn tại hoặc không thuộc thương hiệu của bạn");

        var existingRecipeItem = await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().SingleOrDefaultAsync(
            predicate: x => x.ProductVariantId == request.ProductVariantId 
                            && x.IngredientId == request.IngredientId
        );
        if (existingRecipeItem != null)
            throw new BadHttpRequestException("Thành phần đã tồn tại trong công thức của biến thể sản phẩm này");

        var recipeItem = new Domain.Entities.RecipeItems()
        {
            Id = Guid.CreateVersion7(),
            ProductVariantId = request.ProductVariantId,
            IngredientId = request.IngredientId,
            Quantity = request.Quantity,
            UnitOfMeasureSnapshot = ingredient.MeasureUnit,
            CreatedByAccountId = accountId
        };
        await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().InsertAsync(recipeItem);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Đã xảy ra lỗi khi tạo thành phần công thức");
        }
        _logger.Information("Tạo thành phần công thức cho biến thể sản phẩm {ProductVariantId} thành công", request.ProductVariantId.ToString());
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo thành phần công thức thành công",
            Data = null
        };
    }
}