using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Google.Protobuf.Collections;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Command.UpdateBrandMenuItems;

public class UpdateBrandMenuItemsCommandHandler : IRequestHandler<UpdateBrandMenuItemsCommand, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    public UpdateBrandMenuItemsCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork,
        ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateBrandMenuItemsCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy brandId");
        }
        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy BrandMenu");
        }
        
        var requestedIdStrings = request.UpdateBrandMenuItemsRequest.ProductVariantIds
            .Select(x => x.ToString())
            .ToList();
        var isValidProductVariants = _catalogGrpcService.CheckProductVariantInBrand(
            new CheckProductVariantInBrandRequest()
            {
                BrandId = brandId.ToString(),
                ListProductVariantId = new ListProductVariantId()
                {
                    ProductVariantId =
                    {
                        requestedIdStrings
                    }
                }
            }
        );
        if (!isValidProductVariants.IsValid)
        {
            throw new BadHttpRequestException("Một hoặc nhiều sản phẩm không hợp lệ");
        }
        var existingProductVariants = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
            selector: x =>  x.ProductVariantId,
            predicate: x => x.MenuId == request.BrandMenuId && x.ProductVariantId != null
        );
        
        var existingProductVariantsSet = existingProductVariants.ToHashSet();
        var newProductVariantIds = new HashSet<Guid>(request.UpdateBrandMenuItemsRequest.ProductVariantIds);
        newProductVariantIds.ExceptWith(existingProductVariants);
        var removeProductVariantIds = new HashSet<Guid>(existingProductVariantsSet);
        removeProductVariantIds.ExceptWith(request.UpdateBrandMenuItemsRequest.ProductVariantIds);
        // var newProductVariantIds = request.ProductVariantIds.Except(existingProductVariantsSet).ToList();
        // var removeProductVariantIds = existingProductVariantsSet.Except(request.ProductVariantIds).ToList();
        if (newProductVariantIds.Any())
        {
            var storeMenuAssignmentsList = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetListAsync(
                predicate: x => x.BrandMenuId == request.BrandMenuId
            );
            var newStoreMenuItemAvailability = new List<Domain.Entities.StoreMenuItemAvailability>();
            var newBrandMenuItems = new List<Domain.Entities.BrandMenuItems>();
            foreach (var productVariantId in newProductVariantIds)
            {
                var brandMenuItem = new Domain.Entities.BrandMenuItems()
                {
                    Id = Guid.CreateVersion7(),
                    MenuId = request.BrandMenuId,
                    ProductVariantId = productVariantId,
                    DisplayOrder = 0,
                    Description = null,
                };
                newBrandMenuItems.Add(brandMenuItem);
                
                foreach (var storeMenuAssignment in storeMenuAssignmentsList)
                {
                    var storeMenuItemAvailability = new StoreMenuItemAvailability()
                    {
                        Id = Guid.CreateVersion7(),
                        BrandMenuItemId = brandMenuItem.Id,
                        IsActiveAtStore = false,
                        StoreMenuAssignmentId = storeMenuAssignment.Id,
                    };
                    newStoreMenuItemAvailability.Add(storeMenuItemAvailability);
                }
            }
            //
            // storeMenuAssignmentsList.Select(x => x.StoreMenuItemAvailability)
            //     .ToList()
            //     .AddRange(newStoreMenuItemAvailability);
            await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().InsertRangeAsync(newBrandMenuItems);
            await _unitOfWork.GetRepository<StoreMenuItemAvailability>().InsertRangeAsync(newStoreMenuItemAvailability);
        }

        if (removeProductVariantIds.Any())
        {
            var removeBrandMenuItem = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
                predicate: x => x.MenuId == request.BrandMenuId && removeProductVariantIds.Contains(x.ProductVariantId)
            );
            var storeMenuItemAvailability = await _unitOfWork.GetRepository<StoreMenuItemAvailability>().GetListAsync(
                predicate: x => removeBrandMenuItem.Select(x => x.Id).Contains(x.BrandMenuItemId)
            );
            _unitOfWork.GetRepository<StoreMenuItemAvailability>().DeleteRangeAsync(storeMenuItemAvailability);
            _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().DeleteRangeAsync(removeBrandMenuItem);
        }

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = 200,
                Message = "Cập nhật thành công",
                Data = null
            };
        }
        return new ApiResponse()
        {
            Status = 500,
            Message = "Cập nhật sản phẩm thất bại",
            Data = null
        };
    }
}