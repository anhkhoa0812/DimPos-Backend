using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Enums;
using DimPos.MenuCombo.Domain.Models.BrandMenu;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuById;

public class GetBrandMenuByIdQueryHandler : IRequestHandler<GetBrandMenuByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public GetBrandMenuByIdQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService, StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
        _catalogGrpcService = catalogGrpcService;
        _storeGrpcService = storeGrpcService;
    }
    
    public async ValueTask<ApiResponse> Handle(GetBrandMenuByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("GetBrandMenuByIdQueryHandler.Handle called with request: {@Request}", request);
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu");
        }
        var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId,
            include: x => x.Include(x => x.MenuItems)
        );
        if (brandMenu == null)
        {
            throw new BadHttpRequestException("Không tìm thấy menu của thương hiệu");
        }
        var existingStoreId = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>().GetListAsync(
            selector: x => x.StoreId,
            predicate: x => x.BrandMenuId == brandMenu.Id
        );
        var productVariantsIdInBrandMenu = 
            (brandMenu.MenuItems.Select(x => x.ProductVariantId.ToString())).ToList();
        var productVariantsGrpcResponse = await _catalogGrpcService.GetProductVariantListByIdsAsync(
            new GetProductVariantListByIdsRequest()
            {
                BrandId = brandId.ToString(),
                ProductVariantIds = { productVariantsIdInBrandMenu }
            });
        var storesGrpcResponse = await _storeGrpcService.GetStoresByBrandIdAsync(new GetStoresByBrandIdRequest()
        {
            StoreIds =
            {
                existingStoreId.Select(x => x.ToString()).ToList()
            },
            BrandId = brandId.ToString()
        });
        var brandMenuResponse = new Domain.Models.BrandMenu.BrandMenuByIdResponse()
        {
            Id = brandMenu.Id,
            Name = brandMenu.Name,
            Description = brandMenu.Description,
            IsActiveByBrand = brandMenu.IsActiveByBrand,
            Type = brandMenu.Type,
            ValidFrom = brandMenu.ValidFrom,
            ValidTo = brandMenu.ValidTo,
            ProductVariants = productVariantsGrpcResponse.ProductVariants.Select(x =>
                new BrandMenuByIdResponseWithProductVariants()
                {
                    Id = Guid.Parse(x.Id),
                    Name = x.Name,
                    Price = (decimal)x.Price,
                    Code = x.Code,
                    AlternativeCode = x.AlternativeCode,
                    DiscountPercent = (decimal)x.DiscountPercent,
                    DiscountPrice = (decimal)x.DiscountPrice,
                    PriceCOGS = (decimal)x.PriceCOGS,
                    IsActive = x.IsActive,
                    Size = x.Size,
                    DisplayOrder = x.DisplayOrder,
                    IsMenuDisplay = x.IsMenuDisplay,
                    Status = (EProductVariantStatus)x.Status,
                    Sku = x.Sku
                }).ToList(),
            Stores = storesGrpcResponse.Stores.Select(store => new BrandMenuByIdResponseWithStore()
            {
                Id = Guid.Parse(store.Id),
                Name = store.Name,
                Description = store.Description,
                Address = store.Address,
                Email = store.Email,
                Phone = store.Phone,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                Status = (EStoreStatus) store.Status
            }).ToList()
        };
        _logger.Information("Brand menu found: {@BrandMenuResponse}", brandMenuResponse);
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy thông tin menu thành công",
            Data = brandMenuResponse
        };
    }
}