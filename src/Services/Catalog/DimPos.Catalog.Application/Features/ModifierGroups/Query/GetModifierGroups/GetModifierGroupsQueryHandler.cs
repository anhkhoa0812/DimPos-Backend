using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ModifierGroups;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroups;

public class GetModifierGroupsQueryHandler : IRequestHandler<GetModifierGroupsQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetModifierGroupsQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetModifierGroupsQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var modifierGroups = await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().GetPagingListAsync(
            selector: x => new GetModifierGroupsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                DisplayOrder = x.DisplayOrder,
                Description = x.Description,
                SelectedType = x.SelectedType,
                IsActive = x.IsActive,
                ModifierOptions = x.ModifierOptions != null ? x.ModifierOptions.Select(mo => new ModifierOptionsResponse()
                {
                    Id = mo.Id,
                    Description = mo.Description,
                    IsActive = mo.IsActive,
                    Name = mo.Name,
                    PriceDelta = mo.PriceDelta
                }).ToList() : null
            },
            predicate: x=> x.BrandId == brandId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = modifierGroups
        };
    }
}