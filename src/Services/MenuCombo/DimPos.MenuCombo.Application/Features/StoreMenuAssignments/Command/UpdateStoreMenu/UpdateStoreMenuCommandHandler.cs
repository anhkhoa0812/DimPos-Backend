using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.UpdateStoreMenu;

public class UpdateStoreMenuCommandHandler : IRequestHandler<UpdateStoreMenuCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateStoreMenuCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStoreMenuCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var storeMenuList = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetListAsync(
            predicate: x => x.BrandMenu.BrandId == brandId && x.StoreId == request.StoreId
        );
        if (storeMenuList == null || !storeMenuList.Any())
        {
            throw new BadHttpRequestException("Không tìm thấy menu cho cửa hàng này");
        }
        
        var storeMenu = storeMenuList.FirstOrDefault(x => x.Id == request.StoreMenuId);
        if (storeMenu == null)
        {
            throw new BadHttpRequestException("Menu không tồn tại hoặc không thuộc cửa hàng này");
        }

        if (request.IsActiveAtStore)
        {
            foreach (var storeMenuItem in storeMenuList)
            {
                storeMenuItem.IsActiveAtStore = false;
            }
        }
        else
        {
            if (!storeMenuList.Where(x => x != storeMenu).Any(x => x.IsActiveAtStore))
            {
                throw new BadHttpRequestException("Không thể vô hiệu hóa menu khi không còn menu nào khác đang hoạt động");
            }
        }
        storeMenu.IsActiveAtStore = request.IsActiveAtStore;
        _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().UpdateRange(storeMenuList);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật menu cho cửa hàng không thành công");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật menu cho cửa hàng thành công",
            Data = storeMenu.Id
        };

    }
}