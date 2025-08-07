using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.UpdateIngredient;

public class UpdateIngredientCommandHandler : IRequestHandler<UpdateIngredientCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateIngredientCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");
        
        var ingredient = await _unitOfWork.GetRepository<Domain.Entities.Ingredients>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.IngredientId && x.BrandId == brandId
        );
        
        if (ingredient == null)
            throw new BadHttpRequestException("Thành phần không tồn tại hoặc không thuộc thương hiệu của bạn");
        
        ingredient.Name = request.Name ?? ingredient.Name;
        ingredient.MeasureUnit = request.MeasureUnit ?? ingredient.MeasureUnit;
        ingredient.Description = request.Description ?? ingredient.Description;
        ingredient.IsActive = request.IsActive ?? ingredient.IsActive;
        
        _unitOfWork.GetRepository<Domain.Entities.Ingredients>().UpdateAsync(ingredient);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Đã xảy ra lỗi khi cập nhật thành phần");
        }
        _logger.Information("Cập nhật thành phần {IngredientName} thành công", ingredient.Name);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật thành phần thành công",
            Data = ingredient.Id
        };
    }
}