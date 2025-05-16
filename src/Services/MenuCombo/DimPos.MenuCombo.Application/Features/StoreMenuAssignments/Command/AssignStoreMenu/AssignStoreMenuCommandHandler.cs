using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.AssignStoreMenu;

public class AssignStoreMenuCommandHandler : IRequestHandler<AssignStoreMenuCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;

    public AssignStoreMenuCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }

    public async ValueTask<ApiResponse> Handle(AssignStoreMenuCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }
        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId,
            include: x => x.Include(x => x.MenuItems)
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy BrandMenu");
        }
        var productVariantIds = brandMenu.MenuItems
            .Select(x => x.ProductVariantId)
            .ToList();
        var requestStoreIds = request.AssignStoreMenuRequests.Select(x => x.StoreId).ToHashSet();
        var requestedIdStrings = requestStoreIds
            .Select(x => x.ToString())
            .ToList();
        var isValidStore = _storeGrpcService.CheckStoresInBrand(
            new CheckStoresInBrandRequest()
            {
                BrandId = brandId.ToString(),
                ListStoreId = new ListStoreId()
                {
                    StoreId =
                    {
                        requestedIdStrings
                    }
                }
            }
        );
        if (!isValidStore.IsValid)
        {
            throw new BadHttpRequestException("Một hoặc nhiều cửa hàng không hợp lệ");
        }
        var storeIds = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetListAsync(
            selector: x => x.StoreId,
            predicate: x => x.BrandMenuId == brandMenu.Id
        );
        
        var storeMenusIdSet = storeIds.ToHashSet();
        var newStoreIds = new HashSet<Guid>(requestStoreIds);
        newStoreIds.ExceptWith(storeMenusIdSet);
        var removeStoreIds = new HashSet<Guid>(storeMenusIdSet);
        removeStoreIds.ExceptWith(requestStoreIds);
        if (newStoreIds.Any())
        {
            var storeMenus = newStoreIds.Select(x => new Domain.Entities.StoreMenuAssignments()
            {
                Id = Guid.CreateVersion7(),
                BrandMenuId = brandMenu.Id,
                StoreId = x,
                EffectiveAt = request.AssignStoreMenuRequests.FirstOrDefault(asmr => asmr.StoreId == x )?.EffectiveAt,
                EffectiveEnd = request.AssignStoreMenuRequests.FirstOrDefault(y => y.StoreId == x)?.EffectiveEnd,
                StoreMenuItemAvailability = brandMenu.MenuItems.Select(x => new StoreMenuItemAvailability()
                {
                    Id = Guid.CreateVersion7(),
                    BrandMenuItemId = x.Id,
                    IsActiveAtStore = false,
                }).ToList()
            }).ToList();
            await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().InsertRangeAsync(storeMenus);
            var assignNewStoreMenuRequest = new AssignNewStoreMenuResponse()
            {
                BrandId = brandId.Value,
                // ProductVariantIds = productVariantIds,
                StoreIds = newStoreIds.ToList()
            };
        }
        if (removeStoreIds.Any())
        {
            var storeMenus = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetListAsync(
                predicate: x => removeStoreIds.Contains(x.StoreId) && x.BrandMenuId == brandMenu.Id
            );
            _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().DeleteRangeAsync(storeMenus);
        }
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = 200,
                Message = "Cập nhật thành công"
            };
        }
        return new ApiResponse()
        {
            Status = 500,
            Message = "Một lỗi không xác định đã xảy ra trong quá trình cập nhật"
        };
        
    }
}