using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProductVariantResponse = DimPos.MenuCombo.Domain.Models.StoreMenu.ProductVariantResponse;

namespace DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuById;

public class GetStoreMenuByIdQueryHandler : IRequestHandler<GetStoreMenuByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    
    public GetStoreMenuByIdQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, 
        IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
    }
    public async ValueTask<ApiResponse> Handle(GetStoreMenuByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        
        var storeMenuAssignment = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StoreMenuId && x.BrandMenu.BrandId == brandId,
            include: x => x.Include(x => x.StoreMenuItemAvailability)
                .ThenInclude(x => x.BrandMenuItem)
                .Include(x => x.BrandMenu)
        );
        if (storeMenuAssignment == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thực đơn của cửa hàng");
        }

        var response = new GetStoreMenuByIdResponse()
        {
            Id = storeMenuAssignment.Id,
            IsActiveAtStore = storeMenuAssignment.IsActiveAtStore,
            CreatedDate = storeMenuAssignment.CreatedDate,
            LastModifiedDate = storeMenuAssignment.LastModifiedDate,
            BrandMenu = new BrandMenuForGetStoreMenuById()
            {
                Id = storeMenuAssignment.BrandMenu.Id,
                Name = storeMenuAssignment.BrandMenu.Name,
                Description = storeMenuAssignment.BrandMenu.Description,
                Type = storeMenuAssignment.BrandMenu.Type,
                IsActiveByBrand = storeMenuAssignment.BrandMenu.IsActiveByBrand
            },
        };
        if (storeMenuAssignment.StoreMenuItemAvailability != null)
        {
            var listProductVariantIds = storeMenuAssignment.StoreMenuItemAvailability
                .Select(x => x.BrandMenuItem.ProductVariantId).ToList();
            var productVariantsGrpcResponse = await _catalogGrpcService.GetProductVariantListByIdsForStoreMenuAsync(
                new GetProductVariantListByIdsForStoreMenuRequest()
                {
                    BrandId = brandId.ToString(),
                    ProductVariantIds = { listProductVariantIds.Select(x => x.ToString()) }
                }
            );
            foreach (var storeMenuItem in storeMenuAssignment.StoreMenuItemAvailability)
            {
                var productVariant = productVariantsGrpcResponse.ProductVariants
                    .FirstOrDefault(x => x.Id == storeMenuItem.BrandMenuItem.ProductVariantId.ToString());
                if (productVariant != null)
                {
                    response.StoreMenuItems.Add(new StoreMenuItemForGetStoreMenuById()
                    {
                        Id = storeMenuItem.Id,
                        IsActiveAtStore = storeMenuItem.IsActiveAtStore,
                        CreatedDate = storeMenuItem.CreatedDate,
                        LastModifiedDate = storeMenuItem.LastModifiedDate,
                        ProductVariant = new ProductVariantResponse()
                        {
                            Id = Guid.Parse(productVariant.Id),
                            Name = productVariant.Name,
                            Description = productVariant.Description,
                            Price = (decimal)productVariant.Price,
                            Code = productVariant.Code,
                            IsActive = productVariant.IsActive,
                            Size = productVariant.Size,
                            DisplayOrder = productVariant.DisplayOrder,
                            Sku = productVariant.Sku
                        }
                    });
                }
            }
        }
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };

    }
}