using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Enums;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ProductResponse = DimPos.Catalog.Application.Common.Protos.ProductResponse;
using ProductVariantResponse = DimPos.MenuCombo.Domain.Models.StoreMenu.ProductVariantResponse;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoreMenu;

public class GetStoreMenuQueryHandler : IRequestHandler<GetStoreMenuQuery, ApiResponse>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    public GetStoreMenuQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStoreMenuQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id cửa hàng");
        }
        
        var storeMenuAssignment = await _unitOfWork.GetRepository<Domain.Entities.StoreMenuAssignments>()
            .SingleOrDefaultAsync(
                predicate: sma => sma.IsActiveAtStore == true && sma.StoreId == storeId,
                include: sma => sma.Include(sma => sma.StoreMenuItemAvailability)
            );
        if (storeMenuAssignment == null) 
        {
            throw new BadHttpRequestException("Không tìm thấy Menu của cửa hàng");
        }

        var brandMenuItemIds = storeMenuAssignment.StoreMenuItemAvailability
            .Select(x => x.BrandMenuItemId )
            .ToList();
        var listBrandMenuItems = await _unitOfWork.GetRepository<Domain.Entities.BrandMenuItems>().GetListAsync(
            predicate: x => brandMenuItemIds.Contains(x.Id) && x.ProductVariantId != null
                                                            && x.Menu != null
                                                            && x.Menu.IsActiveByBrand == true,
            include: x => x.Include(x => x.Menu)
        );
        var brandId = listBrandMenuItems.Select(x => x.Menu.BrandId).FirstOrDefault();
        var variantIdStrings = listBrandMenuItems.Select(x => x.ProductVariantId)
            .Select(x => x.ToString())
            .ToList();
        var storeMenuGrpc = await _catalogGrpcService.GetMenuProductByStoreAsync(new GetMenuProductByStoreRequest()
        {
            BrandId = brandId.ToString(),
            StoreId = storeId.ToString(),
            ListProductVariantIds = new ListProductVariantIds()
            {
                ProductVariantId = { variantIdStrings }
            }
        });
        var getTaxRateResponse = await _storeGrpcService.GetTaxRateByStoreIdAsync(new GetTaxRateForStoreMenuRequest()
        {
            StoreId = storeId.ToString(),
            BrandId = brandId.ToString()
        });
        var response = new StoreMenuResponse()
        {
            TaxRate = (decimal) getTaxRateResponse.Rate,
            BrandId = brandId
        };
        var listCategory = new List<CategoriesResponse>();
        foreach (var category in storeMenuGrpc.ListCategoryResponse.Categories)
        {
            var categoryResponse = new CategoriesResponse()
            {
                Id = Guid.Parse(category.Id),
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                Code = category.Code,
                ChildCategories = category.ChildCategories?.ChildCategories.Select(cc => new ChildCategoriesResponse()
                {
                    Id = Guid.Parse(cc.Id),
                    Name = cc.Name,
                    Description = cc.Description,
                    DisplayOrder = cc.DisplayOrder,
                    Code = cc.Code
                }).ToList()
            };
            listCategory.Add(categoryResponse);
        }
        response.Categories = listCategory;
        var listProduct = new List<ProductsResponse>();
        foreach (var product in storeMenuGrpc.ListProductResponse.Products)
        {
            var productResponse = MapProduct(product);
            listProduct.Add(productResponse);
        }
        response.Products = listProduct;
        var listModifierGroup = new List<ModifierGroupsResponses>();
        foreach (var modifierGroup in storeMenuGrpc.ListModifierGroupResponse.ModifierGroups)
        {
            var modifierGroupResponse = MapModifierGroup(modifierGroup);
            listModifierGroup.Add(modifierGroupResponse);
        }
        response.ModifierGroups = listModifierGroup;
        return new ApiResponse()
        {
            Status = 200,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }

    private ProductsResponse MapProduct(ProductResponse product)
    {
        return new ProductsResponse()
        {
            Id = Guid.Parse(product.Id),
            Name = product.Name,
            Code = product.Code,
            Description = product.Description,
            AlternativeCode = product.AlternativeCode,
            ImageUrl = product.ImageUrl,
            Price = (decimal) product.Price,
            CategoryId = Guid.Parse(product.CategoryId),
            ProductVariants = product.ProductVariants?.ProductVariants.Select(pv => new ProductVariantResponse()
            {
                Id = Guid.Parse(pv.Id),
                Code = pv.Code,
                AlternativeCode = pv.AlternativeCode,
                Name = pv.Name,
                DiscountPercent = (decimal) pv.DiscountPercent,
                DiscountPrice = (decimal) pv.DiscountPrice,
                Price = (decimal) pv.Price,
                PriceCOGS = (decimal) pv.PriceCOGS,
                IsActive = pv.IsActive,
                Size = pv.Size,
                IsMenuDisplay = pv.IsMenuDisplay,
                DisplayOrder = pv.DisplayOrder,
                Sku = pv.Sku
            }).ToList()
        };
    }
    private ModifierGroupsResponses MapModifierGroup(ModifierGroupResponse modifierGroup)
    {
        return new ModifierGroupsResponses()
        {
            Id = Guid.Parse(modifierGroup.Id),
            Description = modifierGroup.Description,
            DisplayOrder = modifierGroup.DisplayOrder,
            IsActive = modifierGroup.IsActive,
            ProductVariantIds = modifierGroup.ProductVariantId?.Select(x => Guid.Parse(x)).ToList() ?? new List<Guid>(),
            BrandId = Guid.Parse(modifierGroup.BrandId),
            SelectedType = (ESelectedTypeModifier) modifierGroup.SelectedType,
            ModifierOptions = modifierGroup.ModifierOptions.ModifierOptions.Select(mo => new ModifierOptionsResponses()
            {
                Id = Guid.Parse(mo.Id),
                Name = mo.Name,
                Description = mo.Description,
                IsActive = mo.IsActive,
                PriceDelta = (decimal) mo.PriceDelta,
                ModifierGroupId = Guid.Parse(mo.ModifierGroupId)
            }).ToList()
        };
    }
}