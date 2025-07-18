using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.RemoveRecipeItem;

public class RemoveRecipeItemCommandHandler : IRequestHandler<RemoveRecipeItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public RemoveRecipeItemCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(RemoveRecipeItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");

        var recipeItem = await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.RecipeItemId 
            && x.ProductVariantId == request.ProductVariantId && x.ProductVariant.Product.BrandId == brandId
        );
        if(recipeItem == null)
            throw new BadHttpRequestException("Không tìm thấy thành phần công thức với ID đã cung cấp.");

        _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().DeleteAsync(recipeItem);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Đã xảy ra lỗi khi xóa thành phần công thức");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xóa thành phần công thức thành công",
            Data = null
        };
    }
}