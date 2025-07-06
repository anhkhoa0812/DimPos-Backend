using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.CreateIngredient;

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public CreateIngredientCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");

        var ingredient = new Domain.Entities.Ingredients()
        {
            Id = Guid.CreateVersion7(),
            BrandId = brandId,
            Code = request.Code,
            Sku = request.Sku,
            Name = request.Name,
            MeasureUnit = request.MeasureUnit,
            Description = request.Description,
            IsActive = true
        };
        await _unitOfWork.GetRepository<Domain.Entities.Ingredients>().InsertAsync(ingredient);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Đã xảy ra lỗi khi tạo thành phần");
        }
        _logger.Information("Tạo thành phần {IngredientName} thành công", request.Name);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Tạo thành phần thành công",
            Data = null
        };
    }
}