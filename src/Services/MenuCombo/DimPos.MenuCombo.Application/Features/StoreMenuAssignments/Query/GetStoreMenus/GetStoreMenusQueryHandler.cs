using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenus;

public class GetStoreMenusQueryHandler : IRequestHandler<GetStoreMenusQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStoreMenusQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoreMenusQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if(storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy id của cửa hàng");

        var storeMenus = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetPagingListAsync(
            selector: x => new GetStoreMenusResponse()
            {
                Id = x.Id,
                IsActiveAtStore = x.IsActiveAtStore,
                LastModifiedDate = x.LastModifiedDate,
                CreatedDate = x.CreatedDate,
                BrandMenu = new BrandMenuForGetStoreMenuById()
                {
                    Id = x.BrandMenu.Id,
                    Name = x.BrandMenu.Name,
                    Description = x.BrandMenu.Description,
                    Type = x.BrandMenu.Type,
                    IsActiveByBrand = x.BrandMenu.IsActiveByBrand
                }
            },
            predicate: x => x.StoreId == storeId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thực đơn của cửa hàng thành công",
            Data = storeMenus
        };
    }
}