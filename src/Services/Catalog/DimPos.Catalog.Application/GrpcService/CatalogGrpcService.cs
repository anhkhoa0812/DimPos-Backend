using DimPos.Catalog.Application.Common.Protos;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.GrpcService;

public class CatalogGrpcService : Common.Protos.CatalogGrpcService.CatalogGrpcServiceBase
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;

    public CatalogGrpcService(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<GetProductVariantsByBrandResponse> GetProductVariantsByBrand(
        GetProductVariantsByBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(GetProductVariantsByBrand)} - {TimeUtil.GetCurrentSEATime()}");
        var productVariantsPaging = await _unitOfWork.GetRepository<ProductVariants>().GetPagingListAsync(
            predicate: x => x.Product!.BrandId == Guid.Parse(request.BrandId)
            && x.Product!.Type == EProductType.CustomerOrder,
            page: request.Page,
            size: request.PageSize,
            isAsc: request.IsAsc,
            sortBy: request.SortBy,
            include: x => x.Include(x => x.Product)
        );
        var response = new GetProductVariantsByBrandResponse();
        response.Total = productVariantsPaging.Total;
        response.TotalPages = productVariantsPaging.TotalPages;
        if (productVariantsPaging != null)
        {
            foreach (var productVariant in productVariantsPaging.Items)
            {
                var productVariantResponse = new ProductVariant()
                {
                    Id = productVariant.Id.ToString(),
                    Code = productVariant.Code,
                    Name = productVariant.Name,
                    Description = productVariant.Description ?? String.Empty,
                    DisplayOrder = productVariant.DisplayOrder ?? 0,
                    Price = (float)productVariant.Price,
                    IsActive = productVariant.IsActive,
                    Size = productVariant.Size ?? String.Empty,
                    Sku = productVariant.Sku ?? String.Empty,
                };
                response.ProductVariants.Add(productVariantResponse);
            }
        }

        _logger.Information($"END: {nameof(GetProductVariantsByBrand)} - {TimeUtil.GetCurrentSEATime()}");
        return response;
    }

    public override async Task<CheckProductVariantInBrandResponse> CheckProductVariantInBrand(
        CheckProductVariantInBrandRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(CheckProductVariantInBrand)} - {TimeUtil.GetCurrentSEATime()}");
        var productVariantIds = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            selector: x => x.Id,
            predicate: x => x.Product.BrandId == Guid.Parse(request.BrandId) 
                            && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
        );
        var requestedIds = request.ListProductVariantId.ProductVariantId
            .Select(Guid.Parse)
            .ToList();
        var variantSet = new HashSet<Guid>(productVariantIds);
        bool allExist = requestedIds.All(variantSet.Contains);
        _logger.Information($"END: {nameof(CheckProductVariantInBrand)} - {TimeUtil.GetCurrentSEATime()}");

        if (!allExist)
        {
            return new CheckProductVariantInBrandResponse()
            {
                IsValid = false,
            };
        }

        return new CheckProductVariantInBrandResponse()
        {
            IsValid = true
        };
    }

    public override async Task<GetMenuProductByStoreResponse> GetMenuProductByStore(GetMenuProductByStoreRequest request, ServerCallContext context)
    {
        var variantIds = request.ListProductVariantIds.ProductVariantId.Select(Guid.Parse).ToList();
        var categories = await _unitOfWork.GetRepository<Categories>().GetListAsync(
            predicate: x => x.BrandId == Guid.Parse(request.BrandId) && x.Type == ECategoryType.Parent,
            include: x => x.Include(x => x.ChildCategories)
        );
        var listCategory = new ListCategoryResponse();
        foreach (var category in categories)
        {
            var categoryItem = new CategoryResponse
            {
                Id = category.Id.ToString(),
                Name = category.Name,
                DisplayOrder = category.DisplayOrder ?? 0,
                Code = category.Code ?? String.Empty,
                Description = category.Description ?? String.Empty,
                ChildCategories = category.ChildCategories != null ? new ListChildCategoryResponse()
                {
                    ChildCategories = { category.ChildCategories.Select(x => new ChildCategoryResponse()
                    {
                        Id = x.Id.ToString(),
                        Code = x.Code,
                        Name = x.Name,
                        DisplayOrder = x.DisplayOrder ?? 0,
                        Description = x.Description ?? String.Empty,
                    }) 
                    }
                } : null
            };
            listCategory.Categories.Add(categoryItem);
        }

        var products = await _unitOfWork.GetRepository<Products>().GetListAsync(
            predicate: x => x.BrandId == Guid.Parse(request.BrandId)
                            && x.Type == EProductType.CustomerOrder
                            && x.ProductVariants.Any(pv =>
                                variantIds.Contains(pv.Id) && pv.IsActive),
            include: x => x.Include(x => x.ProductImages)
                .Include(x => x.ProductVariants.Where(pv =>
                    variantIds.Contains(pv.Id) && pv.IsActive))
                .Include(x => x.ProductModifierGroups.Where(pmg => pmg.ModifierGroup.BrandId == Guid.Parse(request.BrandId) 
                && pmg.ModifierGroup.IsActive))
                .ThenInclude(pmg => pmg.ModifierGroup)
                .ThenInclude(mg => mg.ModifierOptions.Where(mo => mo.IsActive)),
            orderBy: x => x.OrderBy(p => p.DisplayOrder)
        );
        if (products.Any(p => p.IsCombo))
        {
            listCategory.Categories.Add(new CategoryResponse()
            {
                Id = Guid.CreateVersion7().ToString(),
                Code = "COMBO",
                Name = "Combo",
                ChildCategories = new ListChildCategoryResponse(),
                Description = "Danh mục dành cho Combo",
                DisplayOrder = 0
            });
        }
        
        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
            predicate: x => x.StoreId == Guid.Parse(request.StoreId)
                           && variantIds.Contains(x.ProductVariantId)
        );
        var comboCategory = listCategory.Categories
            .FirstOrDefault(c => c.Code == "COMBO") ?? null;
        var listProduct = new ListProductResponse()
        {
            Products =
            {
                products.Select(p => MapProduct(p, storePrices.ToList(), comboCategory)).ToList() 
            }
        };
        var productForModifierGroups = products
            .Where(p => p.ProductModifierGroups
                .Any(pmg => pmg.ModifierGroup.IsActive 
                            && pmg.ModifierGroup.ModifierOptions.Any(mo => mo.IsActive)))
            .ToList();
        var listModifierOptions = MapModifierOptions(
            productForModifierGroups.SelectMany(x => x.ProductModifierGroups).ToList()
        );;
        var response = new GetMenuProductByStoreResponse()
        {
            ListCategoryResponse = listCategory,
            ListProductResponse = listProduct,
            ListModifierGroupResponse = listModifierOptions
        };
        return response;
    }
    private ProductResponse MapProduct(Products product, List<StorePrice> storePrices, CategoryResponse? comboCategory = null)
    {
        if (!product.IsHasVariants)
        {
            var variant = product.ProductVariants.FirstOrDefault(pv => pv.ProductId == product.Id);
            var productItem = new ProductResponse()
            {
                Id = variant.Id.ToString(),
                Code = variant.Code,
                Name = variant.Name,
                ImageUrl = product.ProductImages?
                    .SingleOrDefault(x => x.IsMainImage && x.ProductId == product.Id)?.ImageUrl ?? String.Empty,
                Description = product.Description ?? String.Empty,
                Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == variant.Id).OverridePrice,
                CategoryId = product.IsCombo ? comboCategory?.Id : product.CategoryId.ToString(),
                ProductVariants = null
            };
            return productItem;
        }
        else
        {
            var productItem = new ProductResponse()
            {
                Id = product.Id.ToString(),
                Code = product.Code,
                Name = product.Name,
                ImageUrl = product.ProductImages?
                    .SingleOrDefault(x => x.IsMainImage && x.ProductId == product.Id)?.ImageUrl ?? String.Empty,
                Description = product.Description ?? String.Empty,
                Price = 0,
                CategoryId = product.CategoryId.ToString(),
                ProductVariants = new ListProductVariant()
                {
                    ProductVariants =
                    {
                        product.ProductVariants?.OrderBy(pv => pv.DisplayOrder).Select(pv => new ProductVariantResponse()
                        {
                            Id = pv.Id.ToString(),
                            Code = pv.Code,
                            Name = pv.Name,
                            Description = pv.Description ?? String.Empty,
                            DisplayOrder = pv.DisplayOrder ?? 0,
                            Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == pv.Id).OverridePrice,
                            IsActive = pv.IsActive,
                            Size = pv.Size ?? String.Empty,
                            Sku = pv.Sku ?? String.Empty,
                        }).ToList()
                    }
                }
            };
            return productItem;
        }
    }

    private ListModifierGroupResponse MapModifierOptions(List<ProductModifierGroups> productModifierGroups)
    {
        var modifierOptions = new ListModifierGroupResponse();
        foreach (var productModifierGroup in productModifierGroups)
        {
            var modifierGroup = productModifierGroup.ModifierGroup;
            var modifierGroupResponse = new ModifierGroupResponse()
            {
                Id = modifierGroup.Id.ToString(),
                DisplayOrder = modifierGroup.DisplayOrder ?? 0,
                Description = modifierGroup.Description ?? String.Empty,
                SelectedType = (SelectedTypeModifier) modifierGroup.SelectedType,
                IsActive = modifierGroup.IsActive,
                BrandId = modifierGroup.BrandId.ToString(),
                ProductVariantId =
                {
                    productModifierGroup.Product.ProductVariants.Select(x => x.Id.ToString()).ToList(),
                },
                ModifierOptions = new ListModifierOptionResponse()
                {
                    ModifierOptions =
                    {
                        modifierGroup.ModifierOptions?.Select(x => new ModifierOptionResponse()
                        {
                            Id = x.Id.ToString(),
                            Name = x.Name,
                            Description = x.Description ?? String.Empty,
                            IsActive = x.IsActive,
                            PriceDelta = (float)x.PriceDelta,
                            ModifierGroupId = modifierGroup.Id.ToString()
                        })
                    }
                }
            };
            modifierOptions.ModifierGroups.Add(modifierGroupResponse);
        }
        return modifierOptions;
    }

    public override async Task<GetProductVariantListByIdsResponse> GetProductVariantListByIds(GetProductVariantListByIdsRequest request, ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        var variantIds = request.ProductVariantIds
            .Select(Guid.Parse)
            .ToList();

        var productVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            predicate: x => variantIds.Contains(x.Id) 
                            && x.Product.BrandId == brandId 
                            && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
                .ThenInclude(x => x.ProductImages)
        );
        var response = new GetProductVariantListByIdsResponse()
        {
            ProductVariants =
            {
                productVariants.Select(x => new ProductVariantWithImages()
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description ?? String.Empty,
                    DisplayOrder = x.DisplayOrder ?? 0,
                    Price = (float)x.Price,
                    IsActive = x.IsActive,
                    Size = x.Size ?? String.Empty,
                    Sku = x.Sku ?? String.Empty,
                    ProductImages =
                    {
                        x.Product.ProductImages != null ?
                            x.Product.ProductImages.Select(pi => new ProductImage()
                            {
                                Id = pi.Id.ToString(),
                                ImageUrl = pi.ImageUrl,
                                IsMainImage = pi.IsMainImage,
                                AltText = pi.AltText ?? String.Empty,
                            }).ToList() : new List<ProductImage>()
                    }
                }).ToList(),
            }
        };
        
        return response;
    }

    public override async Task<GetProductForOrderResponse> GetProductForOrder(GetProductForOrderRequest request, ServerCallContext context)
    {
        var storeId = Guid.Parse(request.StoreId);
        var brandId = Guid.Parse(request.BrandId);
        var productVariantIds = request.ProductForOrders.Select(x => Guid.Parse(x.Id)).ToList();
        var productVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            predicate: x => productVariantIds.Contains(x.Id)
                            && x.IsActive == true
                            && x.Product.BrandId == brandId && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
                .Include(x => x.Product.ProductModifierGroups.Where(pmg => pmg.ModifierGroup.IsActive))
                .ThenInclude(pmg => pmg.ModifierGroup)
                .ThenInclude(mg => mg.ModifierOptions)
                .Include(x => x.RecipeItems)
                .ThenInclude(ri => ri.Ingredient)
            );
            
        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
            predicate: x => x.StoreId == storeId
                            && productVariantIds.Contains(x.ProductVariantId)
        );
        var storePriceMap = storePrices.ToDictionary(x => x.ProductVariantId);
        var missingVariants = productVariantIds.Except(productVariants.Select(v => v.Id)).ToList();
        if (missingVariants.Any())
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Không tìm thấy Product Variants: {string.Join(',', missingVariants)}"));
        var missingPrices = productVariantIds.Except(storePriceMap.Keys).ToList();
        if (missingPrices.Any())
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Không tìm thấy giá của Product Variant: {string.Join(',', missingPrices)}"));
        var response = new GetProductForOrderResponse();
        foreach (var productForOrder in request.ProductForOrders)
        {
            var id = Guid.Parse(productForOrder.Id);
            var productVariant = productVariants.First(x => x.Id == id);
            var price = storePriceMap[id];
            var productForOrderResponse = new ProductForOrderResponse()
            {
                Id = productVariant.Id.ToString(),
                ProductName = productVariant.Product.Name ?? String.Empty,
                ProductVariantName = productVariant.Name ?? String.Empty,
                UnitPrice = (float) price.OverridePrice,
                Quantity = productForOrder.Quantity,
                Note = productForOrder.Note ?? String.Empty,
                RecipeItems =
                {
                    productVariant.RecipeItems != null ?
                        productVariant.RecipeItems.Select(x => new RecipeItemsForOrderResponse()
                        {
                            RecipeItemId = x.Id.ToString(),
                            Quantity = (float) x.Quantity,
                            Ingredient = new IngredientForOrderResponse()
                            {
                                Id = x.Ingredient.Id.ToString(),
                                Name = x.Ingredient.Name,
                                Sku = x.Ingredient.Sku ?? String.Empty,
                                Code = x.Ingredient.Code ?? String.Empty,
                                MeasureUnit = x.Ingredient.MeasureUnit ?? String.Empty,
                                Description = x.Ingredient.Description ?? String.Empty
                            }
                        }).ToList() : new List<RecipeItemsForOrderResponse>()
                }
            };
            foreach (var modifierOptionId in productForOrder.ModifierOptionIds)
            {
                if (productVariant.Product.ProductModifierGroups != null)
                {
                    var modifierOption = productVariant.Product.ProductModifierGroups
                        .SelectMany(pmg => pmg.ModifierGroup.ModifierOptions)
                        .FirstOrDefault(x => x.Id == Guid.Parse(modifierOptionId));
                    if (modifierOption == null)
                    {
                        throw new RpcException(new Status(StatusCode.NotFound, $"Modifier option with ID {modifierOptionId} not found."));
                    }
                    productForOrderResponse.ModifierOptions.Add(new ModifierOptionForOrderResponse()
                    {
                        Id = modifierOption.Id.ToString(),
                        ModifierGroupId = modifierOption.Id.ToString(),
                        ModifierGroupName = modifierOption.ModifierGroup.Name ?? String.Empty,
                        ModifierOptionName = modifierOption.Name ?? String.Empty,
                        DeltaPrice = (float) modifierOption.PriceDelta
                    });
                }
            }
            response.ProductForOrders.Add(productForOrderResponse);
        }
        return response;
    }

    public override async Task<GetProductVariantForInternalOrderResponse> GetProductVariantForInternalOrder(GetProductVariantForInternalOrderRequest request, ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        var requestProductVariantIds = request.ProductVariantId
            .Select(Guid.Parse)
            .ToList();
        var internalProductVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            predicate: x => requestProductVariantIds.Contains(x.Id)
                            && x.Product.BrandId == brandId
                            && x.Product.Type == EProductType.InternalOrder
                            && x.IsActive == true
                            && x.RecipeItems != null && x.RecipeItems.Any(),
            include: x => 
                x.Include(x => x.Product)
                    .Include(x => x.RecipeItems.Where(x => x.Ingredient.IsActive))
                    .ThenInclude(x => x.Ingredient)
        );
        if (internalProductVariants.Count != request.ProductVariantId.Count)
        {
            throw new RpcException( new Status(StatusCode.NotFound,
                "Một hoặc nhiều Product Variant không tồn tại hoặc không hợp lệ."));
        }
        
        var response = internalProductVariants.Select(x => new ProductVariantForInternalOrder()
        {
            ProductVariantId = x.Id.ToString(),
            ProductVariantName = x.Name.ToString(),
            ProductVariantPrice = (float)x.Price,
            RecipeItems =
            {
                x.RecipeItems?.Select(x => new RecipeItemsForInternalOrder()
                {
                    Id = x.Id.ToString(),
                    IngredientId = x.IngredientId.ToString(),
                    Ingredient = new IngredientForInternalOrder()
                    {
                        Id = x.Ingredient.Id.ToString(),
                        Name = x.Ingredient.Name,
                        Sku = x.Ingredient.Sku ?? String.Empty,
                        UnitOfMeasure = x.Ingredient.MeasureUnit,
                        Description = x.Ingredient.Description ?? String.Empty
                    }
                }).ToList()
            }
        }).ToList();

        return new GetProductVariantForInternalOrderResponse()
        {
            ProductVariants = { response }
        };
    }

    public override async Task<GetProductVariantListByIdsForStoreMenuResponse> GetProductVariantListByIdsForStoreMenu(GetProductVariantListByIdsForStoreMenuRequest request,
        ServerCallContext context)
    {
        var brandId = Guid.Parse(request.BrandId);
        var variantIds = request.ProductVariantIds
            .Select(Guid.Parse)
            .ToList();

        var productVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            predicate: x => variantIds.Contains(x.Id) 
                            && x.Product.BrandId == brandId 
                            && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
        );
        var response = new GetProductVariantListByIdsForStoreMenuResponse();
        foreach (var productVariant in productVariants)
        {
            float overridePrice = 0;
            var storePrice = await _unitOfWork.GetRepository<StorePrice>().SingleOrDefaultAsync(
                predicate: x => x.ProductVariantId == productVariant.Id
            );
            if (storePrice != null)
            {
                overridePrice = (float)storePrice.OverridePrice;
            }
            
            response.ProductVariants.Add(new ProductVariant()
            {
                Id = productVariant.Id.ToString(),
                Code = productVariant.Code,
                Name = productVariant.Name,
                Description = productVariant.Description ?? String.Empty,
                DisplayOrder = productVariant.DisplayOrder ?? 0,
                Price = overridePrice,
                IsActive = productVariant.IsActive,
                Size = productVariant.Size ?? String.Empty,
                Sku = productVariant.Sku ?? String.Empty
            });
        }
        return response;
    }

    public override async Task<GetIngredientsByIngredientIdsResponse> GetIngredientsByIngredientIds(GetIngredientsByIngredientIdsRequest request, ServerCallContext context)
    {
        var ingredientIds = request.IngredientIds
            .Select(Guid.Parse)
            .ToList();
        var ingredients = await _unitOfWork.GetRepository<Ingredients>().GetListAsync(
            predicate: x => ingredientIds.Contains(x.Id)
        );
        if (ingredients.Count != request.IngredientIds.Count)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Một hoặc nhiều nguyên liệu không tồn tại."));
        }
        var response = new GetIngredientsByIngredientIdsResponse()
        {
            Ingredients =
            {
                ingredients.Select(x => new IngredientForGetIngredientsResponse()
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Sku = x.Sku ?? String.Empty,
                    Code = x.Code ?? String.Empty,
                    MeasureUnit = x.MeasureUnit,
                    Description = x.Description ?? String.Empty,
                })
            }
        };
        return response;

    }

    public override async Task<GetRecipeItemsByOrderItemsResponse> GetRecipeItemsByOrderItems(GetRecipeItemsByOrderItemsRequest request, ServerCallContext context)
    {
        var productVariantIds = request.OrderItems.Select(x => Guid.Parse(x.ProductVariantId)).ToList();
        var productVariants = await _unitOfWork.GetRepository<ProductVariants>().GetListAsync(
            predicate: x => productVariantIds.Contains(x.Id)
                            && x.RecipeItems != null && x.RecipeItems.Any(),
            include: x => x.Include(x => x.RecipeItems)
                .ThenInclude(ri => ri.Ingredient)
        );
        var response = new GetRecipeItemsByOrderItemsResponse();
        foreach (var orderItem in request.OrderItems)
        {
            var id = Guid.Parse(orderItem.ProductVariantId);
            var productVariant = productVariants.First(x => x.Id == id);
            if (productVariant.RecipeItems != null)
                response.RecipeItems.Add(productVariant.RecipeItems.Select(x => new RecipeItemsForOrderResponse()
                    {
                        RecipeItemId = x.Id.ToString(),
                        Quantity = orderItem.Quantity * (float) x.Quantity,
                        Ingredient = new IngredientForOrderResponse()
                        {
                            Id = x.Ingredient.Id.ToString(),
                            Name = x.Ingredient.Name,
                            Sku = x.Ingredient.Sku ?? String.Empty,
                            Code = x.Ingredient.Code ?? String.Empty,
                            MeasureUnit = x.Ingredient.MeasureUnit ?? String.Empty,
                            Description = x.Ingredient.Description ?? String.Empty
                        }
                    })
                );
        }
        return response;
    }
    // public override async Task<GetMenuProductByStoreResponse> GetMenuProductByStore(GetMenuProductByStoreRequest request, ServerCallContext context)
    // {
    //     var variantIds = request.ListProductVariantIds.ProductVariantId.Select(Guid.Parse).ToList();
    //     var parentCategories = await _unitOfWork.GetRepository<Categories>().GetListAsync(
    //     predicate: c => c.Status == ECategoryStatus.Active 
    //                  && c.Type == ECategoryType.Parent
    //                  && c.ParentId == null
    //                  && (
    //                     // Nếu là danh mục cha thì kiểm tra xem có sản phẩm nào với variantId không
    //                     c.Products.Any(p => p.ProductVariants.Any(pv =>
    //                         variantIds.Contains(pv.Id)
    //                         && pv.Status == EProductVariantStatus.Active
    //                         && pv.IsActive))
    //                     ||
    //                     // Nếu không có sản phẩm thì kiểm tra xem có danh mục con nào có sản phẩm với variantId không
    //                     c.ChildCategories.Any(cc => 
    //                         cc.Status == ECategoryStatus.Active
    //                         && cc.Products.Any(p => p.ProductVariants.Any(pv =>
    //                             variantIds.Contains(pv.Id)
    //                             && pv.Status == EProductVariantStatus.Active
    //                             && pv.IsActive)))
    //                  ),
    //     include: c => c
    //         .Include(c => c.Products
    //             .Where(p => p.ProductVariants.Any(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive)))
    //         .ThenInclude(p => p.ProductImages)
    //         .Include(c => c.Products
    //             .Where(p => p.ProductVariants.Any(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive)))
    //         .ThenInclude(p => p.ProductVariants
    //             .Where(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive))
    //
    //         .Include(c => c.ChildCategories
    //             .Where(cc => cc.Status == ECategoryStatus.Active
    //                          && cc.Products.Any(p => p.ProductVariants.Any(pv =>
    //                              variantIds.Contains(pv.Id)
    //                              && pv.Status == EProductVariantStatus.Active
    //                              && pv.IsActive))))
    //         .ThenInclude(cc => cc.Products
    //             .Where(p => p.ProductVariants.Any(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive)))
    //         .ThenInclude(p => p.ProductImages)
    //         .Include(c => c.Products
    //             .Where(p => p.ProductVariants.Any(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive)))
    //         .ThenInclude(p => p.ProductVariants
    //             .Where(pv =>
    //                 variantIds.Contains(pv.Id)
    //                 && pv.Status == EProductVariantStatus.Active
    //                 && pv.IsActive))
    // );
    //     var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
    //         predicate: x => x.StoreId == Guid.Parse(request.StoreId)
    //                         && variantIds.Contains(x.ProductVariantId)
    //     );
    //     var response = new GetMenuProductByStoreResponse();
    //     foreach (var parentCategory in parentCategories)
    //     {
    //         response.Response.Add(
    //             MapCategory(parentCategory, storePrices.ToList(), variantIds)
    //         );
    //     }
    //     return response;
    // }
    //
    // private CategoryResponse MapCategory(Categories categories, List<StorePrice> storePrices, List<Guid> variantIds)
    // {
    //     var categoryItem = new CategoryResponse()
    //     {
    //         Id = categories.Id.ToString(),
    //         Name = categories.Name,
    //         DisplayOrder = categories.DisplayOrder ?? 0,
    //         Code = categories.Code ?? String.Empty,
    //         Description = categories.Description ?? String.Empty,
    //     };
    //     if (categories.ChildCategories?.Any() ?? false)
    //     {
    //         var childCategoryList = new ListChildCategoryResponse();
    //         foreach (var childCategory in categories.ChildCategories)
    //         {
    //             childCategoryList.ChildCategories.Add(
    //                 new ChildCategoryResponse()
    //                 {
    //                     Id = childCategory.Id.ToString(),
    //                     Name = childCategory.Name,
    //                     DisplayOrder = childCategory.DisplayOrder ?? 0,
    //                     Code = childCategory.Code ?? String.Empty,
    //                     Description = childCategory.Description ?? String.Empty,
    //                     Products = new ListProductResponse()
    //                     {
    //                         Products =
    //                         {
    //                             childCategory.Products.Select(p => MapProduct(p, storePrices, variantIds))
    //                         }
    //                     }
    //                 }
    //             );
    //         }
    //         categoryItem.ChildCategories = childCategoryList;
    //     }
    //     else
    //     {
    //         var productList = new ListProductResponse();
    //         foreach (var product in categories.Products)
    //         {
    //             productList.Products.Add(
    //                 MapProduct(product, storePrices, variantIds)
    //                 );
    //         }
    //         categoryItem.Products = productList;
    //     }
    //     return categoryItem;
    // }
    //
    // public override async Task<GetMenuProductByStoreResponse> GetMenuProductByStore(GetMenuProductByStoreRequest request, ServerCallContext context)
    // {
    //     var variantIds = request.ListProductVariantIds.ProductVariantId.Select(Guid.Parse).ToList();
    //     var categories = await _unitOfWork.GetRepository<Categories>().GetListAsync(
    //         predicate: x =>
    //             x.Status == ECategoryStatus.Active &&
    //             x.Products.Any(p => p.ProductVariants
    //                 .Any(v => variantIds.Contains(v.Id) && v.Status == EProductVariantStatus.Active && v.IsActive == true)),
    //         include: x => x.Include(c => c.Products.Where(p => p.ProductVariants.Any(v => variantIds.Contains(v.Id))))
    //             .ThenInclude(p => p.ProductVariants.Where(v => variantIds.Contains(v.Id)))
    //     );
    //     var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
    //         predicate: x => x.StoreId == Guid.Parse(request.StoreId)
    //         && variantIds.Contains(x.ProductVariantId)
    //     );
    //     var response = new GetMenuProductByStoreResponse();
    //     foreach (var category in categories)
    //     {
    //         var categoryItem = new CategoryResponse()
    //         {
    //             Id = category.Id.ToString(),
    //             Name = category.Name,
    //             DisplayOrder = category.DisplayOrder ?? 0,
    //             Code = category.Code ?? String.Empty,
    //             Description = category.Description ?? String.Empty,
    //         };
    //         foreach (var product in category.Products)
    //         {
    //             if (!product.IsHasVariants)
    //             {
    //                 var variant = product.ProductVariants.FirstOrDefault(pv => pv.ProductId == product.Id);
    //                 var productItem = new ProductResponse()
    //                 {
    //                     Id = variant.Id.ToString(),
    //                     Code = variant.Code,
    //                     Name = variant.Name,
    //                     AlternativeCode = variant.AlternativeCode ?? String.Empty,
    //                     ImageUrl = String.Empty,
    //                     Description = product.Description,
    //                     Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == variant.Id).OverridePrice,
    //                     ProductVariants = null
    //                 };
    //                 categoryItem.Products.Add(productItem);
    //             }
    //             else
    //             {
    //                 var productItem = new ProductResponse()
    //                 {
    //                     Id = product.Id.ToString(),
    //                     Code = product.Code,
    //                     Name = product.Name,
    //                     AlternativeCode = product.AlternativeCode ?? String.Empty,
    //                     ImageUrl = "",
    //                     Description = product.Description,
    //                     Price = 0,
    //                     ProductVariants = new ListProductVariant()
    //                     {
    //                         ProductVariants =
    //                         {
    //                             product.ProductVariants?.Select(pv => new ProductVariantResponse()
    //                             {
    //                                 Id = pv.Id.ToString(),
    //                                 Code = pv.Code,
    //                                 Name = pv.Name,
    //                                 AlternativeCode = pv.AlternativeCode ?? String.Empty,
    //                                 DisplayOrder = pv.DisplayOrder ?? 0,
    //                                 DiscountPercent = (float) pv.DiscountPercent,
    //                                 DiscountPrice = (float)pv.DiscountPrice,
    //                                 Price = (float) storePrices.FirstOrDefault(x => x.ProductVariantId == pv.Id).OverridePrice,
    //                                 PriceCOGS = (float)pv.PriceCOGS,
    //                                 IsActive = pv.IsActive,
    //                                 IsMenuDisplay = pv.IsMenuDisplay ?? false
    //                             }).ToList()
    //                         }
    //                     }
    //                 };
    //                 categoryItem.Products.Add(productItem);
    //             }
    //         }
    //
    //         response.Response.Add(categoryItem);
    //     }
    //     return response;
    // }
}
