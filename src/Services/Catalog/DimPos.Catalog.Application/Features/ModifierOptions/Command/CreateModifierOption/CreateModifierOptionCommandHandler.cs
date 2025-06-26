using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.CreateModifierOption;

public class CreateModifierOptionCommandHandler : IRequestHandler<CreateModifierOptionCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreateModifierOptionCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    
    public async ValueTask<ApiResponse> Handle(CreateModifierOptionCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var modifierGroup = await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ModifierGroupId && x.BrandId == brandId
        );
        if (modifierGroup == null)
        {
            _logger.Warning("Modifier group with ID {ModifierGroupId} not found for brand {BrandId}", request.ModifierGroupId, brandId);
            throw new BadHttpRequestException("Không tìm thấy nhóm tùy chọn");
        }

        if (!modifierGroup.IsActive)
        {
            _logger.Warning("Modifier group with ID {ModifierGroupId} is not active", request.ModifierGroupId);
            throw new BadHttpRequestException("Nhóm tùy chọn không hoạt động");
        }
        
        var modifierOption = new Domain.Entities.ModifierOptions
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            PriceDelta = request.PriceDelta,
            ModifierGroupId = request.ModifierGroupId
        };
        await _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().InsertAsync(modifierOption);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to create modifier option for group {ModifierGroupId}", request.ModifierGroupId);
            throw new BadHttpRequestException("Tạo tùy chọn không thành công");
        }
        _logger.Information("Successfully created modifier option with ID {ModifierOptionId} for group {ModifierGroupId}", modifierOption.Id, request.ModifierGroupId);
        return new ApiResponse
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo tùy chọn thành công",
            Data = null
        };
    }
}