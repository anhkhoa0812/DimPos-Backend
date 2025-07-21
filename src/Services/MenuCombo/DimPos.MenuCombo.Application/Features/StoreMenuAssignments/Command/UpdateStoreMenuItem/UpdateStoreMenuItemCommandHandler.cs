using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.UpdateStoreMenuItem;

public class UpdateStoreMenuItemCommandHandler : IRequestHandler<UpdateStoreMenuItemCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateStoreMenuItemCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStoreMenuItemCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }

        var storeMenu = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StoreMenuItem && x.BrandMenu.BrandId == brandId,
            include: x => x.Include(x => x.StoreMenuItemAvailability)
                .ThenInclude(x => x.BrandMenuItem)
        );

        if (storeMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thực đơn của cửa hàng");
        }

        var existingProductVariants = storeMenu.StoreMenuItemAvailability != null
            ? storeMenu.StoreMenuItemAvailability.Select(x => x.BrandMenuItem.ProductVariantId).ToList()
            : new List<Guid>();
        var newProductVariantIds = request.ProductVariantIds != null ?
            request.ProductVariantIds.Where(x => !existingProductVariants.Contains(x))
                .ToList() : new List<Guid>();
        var removedProductVariantIds = request.ProductVariantIds != null
            ? existingProductVariants
                .Where(x => request.ProductVariantIds == null || !request.ProductVariantIds.Contains(x))
                .ToList()
            : existingProductVariants;
        
        if(!newProductVariantIds.Any() && !removedProductVariantIds.Any())
        {
            throw new BadHttpRequestException("Không có sản phẩm nào được thêm hoặc xóa");
        }

        if (newProductVariantIds.Any())
        {
            foreach (var newProductVariantId in newProductVariantIds)
            {
                var brandMenuItem = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>()
                    .SingleOrDefaultAsync(
                        predicate: x => x.ProductVariantId == newProductVariantId && x.Menu.BrandId == brandId
                    );
                if (brandMenuItem == null)
                {
                    throw new BadHttpRequestException("Không tìm thấy Brand Menu Item cho sản phẩm: " + newProductVariantId);
                }
                var newStoreMenuItem = new StoreMenuItemAvailability()
                {
                    Id = Guid.CreateVersion7(),
                    BrandMenuItemId = brandMenuItem.Id,
                    IsActiveAtStore = true,
                    StoreMenuAssignmentId = storeMenu.Id,
                };
                await _unitOfWork.GetRepository<StoreMenuItemAvailability>().InsertAsync(newStoreMenuItem);
            }
        }

        if (removedProductVariantIds.Any())
        {
            if (storeMenu.StoreMenuItemAvailability != null)
            {
                var removedStoreMenuItems = storeMenu.StoreMenuItemAvailability
                    .Where(x => removedProductVariantIds.Contains(x.BrandMenuItem.ProductVariantId))
                    .ToList();
                _unitOfWork.GetRepository<StoreMenuItemAvailability>().DeleteRangeAsync(removedStoreMenuItems);
            }
        }
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật Store Menu Item không thành công, vui lòng thử lại sau");
        }
        return new ApiResponse
        {
            Status = 200,
            Message = "Cập nhật Store Menu Item thành công",
            Data = storeMenu.Id
        };
    }
}