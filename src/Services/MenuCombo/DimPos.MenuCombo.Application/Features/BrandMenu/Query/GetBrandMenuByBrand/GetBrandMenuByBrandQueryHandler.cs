using DimPos.MenuCombo.Application.Common.Mapper;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuByBrand;

public class GetBrandMenuByBrandQueryHandler : IRequestHandler<GetBrandMenuByBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    public GetBrandMenuByBrandQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(GetBrandMenuByBrandQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }
        var brandMenus = await _unitOfWork
            .GetRepository<Domain.Entities.BrandMenu>()
            .GetPagingListAsync(
                predicate: x => x.BrandId == brandId, 
                page: request.Page,
                size: request.Size,
                filter: null,
                sortBy: request.SortBy,
                isAsc: request.IsAsc
            );
        var response = BrandMenuMapper.ToBrandMenuResponsePaginate(brandMenus);
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy danh sách menu thành công",
            Data = response
        };
    }
}