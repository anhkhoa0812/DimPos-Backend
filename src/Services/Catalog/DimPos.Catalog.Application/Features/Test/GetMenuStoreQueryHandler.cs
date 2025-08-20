using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Test;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.Test;

public class GetMenuStoreQueryHandler : IRequestHandler<GetMenuStoreQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public GetMenuStoreQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async ValueTask<ApiResponse> Handle(GetMenuStoreQuery request, CancellationToken cancellationToken)
    {

        var variantIds = request.ProductVariantIds;
        var categories = await _unitOfWork.GetRepository<Domain.Entities.Categories>().GetListAsync(
            predicate: x => x.BrandId == request.BrandId && x.Type == ECategoryType.Parent,
            include: x => x.Include(x => x.ChildCategories),
            orderBy: x => x.OrderBy(x => x.DisplayOrder)
        );
        var categoryResponses = categories.Select(category => new CategoriesResponse()
        {
            Id = category.Id,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder ?? 0,
            Code = category.Code ?? String.Empty,
            Description = category.Description ?? String.Empty,
            ChildCategories = category.ChildCategories != null
                ? category.ChildCategories.Select(x => new ChildCategoriesResponse()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    DisplayOrder = x.DisplayOrder ?? 0,
                    Description = x.Description ?? String.Empty, 
                }).ToList() : null
        }).ToList();
        
        var products = await _unitOfWork.GetRepository<Domain.Entities.Products>().GetListAsync(
            predicate: x => x.BrandId == request.BrandId
                            && x.Type == EProductType.CustomerOrder
                            && x.ProductVariants.Any(pv =>
                                variantIds.Contains(pv.Id) && pv.IsActive),
            include: x => x.Include(x => x.ProductImages.Where(img => img.IsMainImage))
                .Include(x => x.ProductModifierGroups)
                .Include(x => x.ProductVariants.Where(pv => variantIds.Contains(pv.Id) && pv.IsActive))
                .Include(x => x.ProductComboItems)
                .ThenInclude(x => x.ItemProductVariant),
                // .ThenInclude(x => x.Product)
                // .ThenInclude(x => x.ProductModifierGroups),
            orderBy: x => x.OrderBy(p => p.DisplayOrder)
        );
        var comboItemVariantIds = products
            .Where(p => p.IsCombo)
            .SelectMany(p => p.ProductComboItems)
            .Select(pci => pci.ItemProductVariantId)
            .Distinct()
            .ToList();
        var allVariantIds = variantIds.Union(comboItemVariantIds).ToList();
       
        if (products.Any(p => p.IsCombo))
        {
            categoryResponses.Add(new CategoriesResponse()
            {
                Id = Guid.CreateVersion7(),
                Code = "COMBO",
                Name = "Combo",
                ChildCategories = null,
                Description = "Danh mục dành cho Combo",
                DisplayOrder = 0
            });
        }

        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
            predicate: x => x.StoreId == request.StoreId
                           && variantIds.Contains(x.ProductVariantId)
        );
        var priceDictionary = storePrices.ToDictionary(p => p.ProductVariantId, p => p.OverridePrice);

        var comboCategory = categoryResponses
            .FirstOrDefault(c => c.Code == "COMBO") ?? null;

        var listProduct = products.Select(p => MapProduct(p, priceDictionary, comboCategory)).ToList();
        
        var modifierGroupIds = new HashSet<Guid>();
        foreach (var product in products)
        {
            foreach (var pmg in product.ProductModifierGroups)
            {
                modifierGroupIds.Add(pmg.ModifierGroupId);
            }
        }

        if (comboItemVariantIds.Any())
        {
            var comboModifierGroups = await _unitOfWork.GetRepository<ProductModifierGroups>().GetListAsync(
                predicate: pmg => pmg.Product.ProductVariants.Any(pv => comboItemVariantIds.Contains(pv.Id))
            );
            
            foreach (var pmg in comboModifierGroups)
            {
                modifierGroupIds.Add(pmg.ModifierGroupId);
            }
        }

        var modifierGroups = modifierGroupIds.Any() ? await _unitOfWork.GetRepository<Domain.Entities.ModifierGroups>().GetListAsync(
            predicate: mg => modifierGroupIds.Contains(mg.Id) 
                             && mg.BrandId == request.BrandId
                             && mg.IsActive,
            include: mg => mg.Include(x => x.ModifierOptions.Where(mo => mo.IsActive))
                .Include(x => x.ProductModifierGroups)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.ProductVariants.Where(pv => allVariantIds.Contains(pv.Id)))
        ) : new List<Domain.Entities.ModifierGroups>();
        
        _logger.Information(
            $"Found {modifierGroups.Count} active modifier groups for brand {request.BrandId} with IDs: {string.Join(", ", modifierGroups.Select(mg => mg.Id))}"
        );
        var listModifierOptions = MapModifierOptions(
            modifierGroups.ToList()
        );
        var response = new StoreMenuResponse()
        {
            Categories = categoryResponses,
            Products = listProduct,
            ModifierGroups = listModifierOptions,
            BrandId = request.BrandId,
            TaxRate = 0
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin menu cửa hàng thành công",
            Data = response
        };
    }
     private ProductsResponse MapProduct(Domain.Entities.Products product,  Dictionary<Guid,decimal> priceDictionary, CategoriesResponse? comboCategory = null)
    {
        if (!product.IsHasVariants)
        {
            var variant = product.ProductVariants.FirstOrDefault(pv => pv.ProductId == product.Id);
            var productItem = new ProductsResponse()
            {
                Id = variant.Id,
                Code = variant.Code,
                Name = variant.Name,
                ImageUrl = product.ProductImages?
                    .SingleOrDefault(x => x.IsMainImage && x.ProductId == product.Id)?.ImageUrl ?? String.Empty,
                Description = product.Description ?? String.Empty,
                Price = priceDictionary[variant.Id],
                CategoryId = product.IsCombo ? comboCategory.Id : product.CategoryId,
                ProductVariants = null,
                ComboItems = product.IsCombo ? 
                
                        product.ProductComboItems.OrderBy(pci => pci.DisplayOrder ?? 0).Select(pci => new ComboItemsResponse()
                        {
                            Id = pci.Id,
                            DisplayOrder = pci.DisplayOrder ?? 0,
                            Quantity = pci.Quantity,
                            ProductVariant = new ProductVariantResponse()
                            {
                                Id = pci.ItemProductVariant.Id,
                                Code = pci.ItemProductVariant.Code,
                                Name = pci.ItemProductVariant.Name,
                                Description = pci.ItemProductVariant.Description ?? String.Empty,
                                DisplayOrder = pci.ItemProductVariant.DisplayOrder ?? 0,
                                Price = 0,
                                IsActive = pci.ItemProductVariant.IsActive,
                                Size = pci.ItemProductVariant.Size ?? String.Empty,
                                Sku = pci.ItemProductVariant.Sku ?? String.Empty,
                            }
                        }).ToList() 
                : null
            };
            return productItem;
        }
        else
        {
            var productItem = new ProductsResponse()
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                ImageUrl = product.ProductImages?
                    .SingleOrDefault(x => x.IsMainImage && x.ProductId == product.Id)?.ImageUrl ?? String.Empty,
                Description = product.Description ?? String.Empty,
                Price = 0,
                CategoryId = product.CategoryId,
                ProductVariants = 
                        product.ProductVariants?.OrderBy(pv => pv.DisplayOrder).Select(pv => new ProductVariantResponse()
                        {
                            Id = pv.Id,
                            Code = pv.Code,
                            Name = pv.Name,
                            Description = pv.Description ?? String.Empty,
                            DisplayOrder = pv.DisplayOrder ?? 0,
                            Price = priceDictionary[pv.Id],
                            IsActive = pv.IsActive,
                            Size = pv.Size ?? String.Empty,
                            Sku = pv.Sku ?? String.Empty,
                        }).ToList(),
            };
            return productItem;
        }
    }

    private List<ModifierGroupsResponses> MapModifierOptions(
        List<Domain.Entities.ModifierGroups> modifierGroups)
    {
        return modifierGroups.Select(modifierGroup => new ModifierGroupsResponses()
        {
            Id = modifierGroup.Id,
            Name = modifierGroup.Name,
            DisplayOrder = modifierGroup.DisplayOrder ?? 0,
            Description = modifierGroup.Description ?? string.Empty,
            SelectedType = modifierGroup.SelectedType,
            IsActive = modifierGroup.IsActive,
            BrandId = modifierGroup.BrandId,
            ProductVariantIds = modifierGroup.ProductModifierGroups
                .SelectMany(pmg => pmg.Product.ProductVariants.Select(pv => pv.Id))
                .Distinct()
                .ToList(),
            ModifierOptions = modifierGroup.ModifierOptions?.Select(option => new ModifierOptionsResponses()
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Description ?? string.Empty,
                IsActive = option.IsActive,
                PriceDelta = 0,
                ModifierGroupId = modifierGroup.Id
            }).ToList()
        }).ToList();
    }
}