using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ModifierGroups;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroupsById;

public class GetModifierGroupsByIdQueryHandler : IRequestHandler<GetModifierGroupsByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetModifierGroupsByIdQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetModifierGroupsByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        var modifierGroup = await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().SingleOrDefaultAsync(
            selector: x => new GetModifierGroupsResponse()
            {
                Id = x.Id,
                Name = x.Name,
                DisplayOrder = x.DisplayOrder,
                Description = x.Description,
                SelectedType = x.SelectedType,
                IsActive = x.IsActive,
                ModifierOptions = x.ModifierOptions != null ? x.ModifierOptions.Select(x => new ModifierOptionsResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    PriceDelta = x.PriceDelta
                }).ToList() : null
            },
            predicate: x => x.Id == request.ModifierGroupId && x.BrandId == brandId
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = modifierGroup
        };
    }
}