using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuByStoreId;

public class GetStoreMenuByStoreIdQueryHandler : IRequestHandler<GetStoreMenuByStoreIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStoreMenuByStoreIdQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoreMenuByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var storeMenuAssignments = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>()
            .GetPagingListAsync(
                selector: x => new GetStoreMenuByStoreIdResponse()
                {
                    Id = x.Id,
                    IsActiveAtStore = x.IsActiveAtStore,
                    CreatedDate = x.CreatedDate,
                    LastModifiedDate = x.LastModifiedDate,
                    BrandMenu = new BrandMenuForGetStoreMenuByStoreIdResponse()
                    {
                        Id = x.BrandMenu.Id,
                        Name = x.BrandMenu.Name,
                        Description = x.BrandMenu.Description,
                        Type = x.BrandMenu.Type,
                        IsActiveByBrand = x.BrandMenu.IsActiveByBrand,
                    }
                }, 
                predicate: x => x.StoreId == request.StoreId 
                                   && x.BrandMenu.BrandId == brandId,
                page: request.Page,
                size: request.Size,
                sortBy: request.SortBy ?? "CreatedDate",
                isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy danh sách menu của cửa hàng thành công",
            Data = storeMenuAssignments
        };
    }
}