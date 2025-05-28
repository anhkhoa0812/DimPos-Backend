using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ModifierOptions;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsById;

public class GetModifierOptionsByIdQueryHandler : IRequestHandler<GetModifierOptionsByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetModifierOptionsByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetModifierOptionsByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        _logger.Information("BEGIN: GetModifierOptionsByIdQueryHandler.Handle - ModifierOptionId: {ModifierOptionId}", request.ModifierOptionId);
        var modifierOption = await _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().SingleOrDefaultAsync(
            selector: x => new GetModifierOptionsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                PriceDelta = x.PriceDelta,
            },
            predicate: x => x.Id == request.ModifierOptionId && x.ModifierGroup.BrandId == brandId
        );
        if (modifierOption == null)
        {
            _logger.Error("Modifier option with ID not found");
            throw new NotFoundException("Không tìm thấy tùy chọn tùy chỉnh với ID đã cung cấp.");
        }
        _logger.Information("END: GetModifierOptionsByIdQueryHandler.Handle - ModifierOptionId: {ModifierOptionId}", request.ModifierOptionId);
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = modifierOption
        };
    }
}