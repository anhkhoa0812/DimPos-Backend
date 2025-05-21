using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
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
    public GetStoreMenuQueryHandler(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
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
        var response = new StoreMenuResponse();
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
                DisplayOrder = pv.DisplayOrder
            }).ToList()
        };
    }
}