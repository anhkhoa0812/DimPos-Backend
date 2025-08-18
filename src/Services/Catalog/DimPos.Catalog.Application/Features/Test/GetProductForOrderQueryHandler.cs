using DimPos.Catalog.Application.Common.Protos;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Application.Features.Test;

public class GetProductForOrderQueryHandler : IRequestHandler<GetProductForOrderQuery, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public GetProductForOrderQueryHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async ValueTask<ApiResponse> Handle(GetProductForOrderQuery request, CancellationToken cancellationToken)
    {
        var storeId = Guid.Parse(request.StoreId);
        var brandId = Guid.Parse(request.BrandId);
        var productVariantIds = request.ProductForOrders.Select(x => Guid.Parse(x.Id)).ToList();
        
        // Load only essential data for product variants first
        var productVariants = await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetListAsync(
            predicate: x => productVariantIds.Contains(x.Id)
                            && x.IsActive == true
                            && x.Product.BrandId == brandId 
                            && x.Product.Type == EProductType.CustomerOrder,
            include: x => x.Include(x => x.Product)
        );
        
        var response = new GetProductForOrderResponse();
        
        var missingVariants = productVariantIds.Except(productVariants.Select(v => v.Id)).ToList();
        if (missingVariants.Any())
        {
            return new ApiResponse()
            {
                Status = 400,
                Message = "",
            };
        }
        
        var productIds = productVariants.Select(pv => pv.ProductId).Distinct().ToList();
        var foundVariantIds = productVariants.Select(pv => pv.Id).ToList();
        
        // Load store prices
        var storePrices = await _unitOfWork.GetRepository<StorePrice>().GetListAsync(
            predicate: x => x.StoreId == storeId && foundVariantIds.Contains(x.ProductVariantId)
        );
        var storePriceMap = storePrices.ToDictionary(x => x.ProductVariantId);
        
        var missingPrices = foundVariantIds.Except(storePriceMap.Keys).ToList();
        if (missingPrices.Any())
        {
            response.IsSuccess = false;
            response.ErrorMessage = $"Không tìm thấy giá của những biến thể sản phẩm: {string.Join(',', missingPrices)}";
        }
        
        // Load recipe items in separate query
        var recipeItems = await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().GetListAsync(
            predicate: x => foundVariantIds.Contains(x.ProductVariantId),
            include: x => x.Include(ri => ri.Ingredient)
        );
        var recipeItemsLookup = recipeItems.GroupBy(ri => ri.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());
        
        // Identify combo vs non-combo products
        var comboProducts = productVariants.Where(pv => pv.Product.IsCombo).Select(pv => pv.ProductId).ToList();
        var nonComboProducts = productVariants.Where(pv => !pv.Product.IsCombo).Select(pv => pv.ProductId).ToList();
        
        // Load combo items only for combo products (simplified query)
        var comboItems = comboProducts.Any() 
            ? await _unitOfWork.GetRepository<Domain.Entities.ProductComboItems>().GetListAsync(
                predicate: x => comboProducts.Contains(x.ProductId)
            )
            : new List<Domain.Entities.ProductComboItems>();
        var comboItemsLookup = comboItems.GroupBy(ci => ci.ProductId).ToDictionary(g => g.Key, g => g.ToList());
        
        // Get combo item variant IDs
        var comboItemVariantIds = comboItems.Select(ci => ci.ItemProductVariantId).Distinct().ToList();
        
        // Load combo item variants (only names needed)
        var comboItemVariants = comboItemVariantIds.Any()
            ? await _unitOfWork.GetRepository<Domain.Entities.ProductVariants>().GetListAsync(
                predicate: pv => comboItemVariantIds.Contains(pv.Id)
            )
            : new List<Domain.Entities.ProductVariants>();
        var comboItemVariantsLookup = comboItemVariants.ToDictionary(x => x.Id);
        
        // Load combo recipe items
        var comboRecipeItems = comboItemVariantIds.Any() 
            ? await _unitOfWork.GetRepository<Domain.Entities.RecipeItems>().GetListAsync(
                predicate: x => comboItemVariantIds.Contains(x.ProductVariantId),
                include: x => x.Include(ri => ri.Ingredient)
            )
            : new List<Domain.Entities.RecipeItems>();
        var comboRecipeLookup = comboRecipeItems.GroupBy(ri => ri.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());
        
        // Load modifier groups for non-combo products only
        var productModifierGroups = nonComboProducts.Any() 
            ? await _unitOfWork.GetRepository<ProductModifierGroups>().GetListAsync(
                predicate: pmg => nonComboProducts.Contains(pmg.ProductId) && pmg.ModifierGroup.IsActive,
                include: pmg => pmg.Include(x => x.ModifierGroup)
                    .ThenInclude(mg => mg.ModifierOptions.Where(mo => mo.IsActive))
            )
            : new List<ProductModifierGroups>();
        var productModifierLookup = productModifierGroups.GroupBy(pmg => pmg.ProductId).ToDictionary(g => g.Key, g => g.ToList());
        var productVariantLookup = productVariants.ToDictionary(x => x.Id);
        // For combo modifiers, only load what's actually needed based on request
        var requestedComboModifierOptionIds = request.ProductForOrders
            .Where(p => productVariantLookup.ContainsKey(Guid.Parse(p.Id)) && 
                       productVariantLookup[Guid.Parse(p.Id)].Product.IsCombo)
            .SelectMany(p => p.ModifierOptions.Select(mo => Guid.Parse(mo.ModifierOptionId)))
            .Distinct()
            .ToList();
            
        var comboModifierOptions = requestedComboModifierOptionIds.Any()
            ? await _unitOfWork.GetRepository<Domain.Entities.ModifierOptions>().GetListAsync(
                predicate: mo => requestedComboModifierOptionIds.Contains(mo.Id) && mo.IsActive,
                include: mo => mo.Include(x => x.ModifierGroup)
            )
            : new List<Domain.Entities.ModifierOptions>();
        var comboModifierOptionsLookup = comboModifierOptions.ToDictionary(x => x.Id);
        
        
        foreach (var productForOrder in request.ProductForOrders)
        {
            var id = Guid.Parse(productForOrder.Id);
            var productVariant = productVariantLookup[id];
            var price = storePriceMap[id];

            var productForOrderResponse = new ProductForOrderResponse()
            {
                Id = productVariant.Id.ToString(),
                ProductName = productVariant.Product.Name ?? string.Empty,
                ProductVariantName = productVariant.Name ?? string.Empty,
                UnitPrice = (float)price.OverridePrice,
                Quantity = productForOrder.Quantity,
                RecipeItems = {
                    recipeItemsLookup.TryGetValue(id, out var recipeList) 
                        ? recipeList.Select(x => new RecipeItemsForOrderResponse()
                        {
                            RecipeItemId = x.Id.ToString(),
                            Quantity = (float)x.Quantity,
                            Ingredient = new IngredientForOrderResponse()
                            {
                                Id = x.Ingredient.Id.ToString(),
                                Name = x.Ingredient.Name,
                                Sku = x.Ingredient.Sku ?? string.Empty,
                                Code = x.Ingredient.Code ?? string.Empty,
                                MeasureUnit = x.Ingredient.MeasureUnit ?? string.Empty,
                                Description = x.Ingredient.Description ?? string.Empty
                            }
                        }).ToList()
                        : new List<RecipeItemsForOrderResponse>()
                }
            };
            
            // Add combo recipe items
            if (productVariant.Product.IsCombo && comboItemsLookup.TryGetValue(productVariant.ProductId, out var comboItemList))
            {
                var comboItemIds = comboItemList.Select(x => x.ItemProductVariantId).ToList();
                foreach (var comboItemId in comboItemIds)
                {
                    if (comboRecipeLookup.TryGetValue(comboItemId, out var recipes))
                    {
                        productForOrderResponse.RecipeItems.AddRange(
                            recipes.Select(x => new RecipeItemsForOrderResponse()
                            {
                                RecipeItemId = x.Id.ToString(),
                                Quantity = (float)x.Quantity,
                                Ingredient = new IngredientForOrderResponse()
                                {
                                    Id = x.Ingredient.Id.ToString(),
                                    Name = x.Ingredient.Name,
                                    Sku = x.Ingredient.Sku ?? string.Empty,
                                    Code = x.Ingredient.Code ?? string.Empty,
                                    MeasureUnit = x.Ingredient.MeasureUnit ?? string.Empty,
                                    Description = x.Ingredient.Description ?? string.Empty
                                }
                            }).ToList()
                        );
                    }
                }
            }
            
            var modifierOptionRequests = productForOrder.ModifierOptions
                .Select(mo => new 
                {
                    ModifierOptionId = Guid.Parse(mo.ModifierOptionId),
                    RelatedComboProductVariantItemId = !string.IsNullOrEmpty(mo.RelatedComboProductVariantItemId) 
                        ? Guid.Parse(mo.RelatedComboProductVariantItemId) 
                        : (Guid?)null,
                    Original = mo
                }).ToList();
                
            foreach (var modifierOptionRequest in modifierOptionRequests)
            {
                if (!productVariant.Product.IsCombo)
                {
                    var modifierOption = productModifierLookup.TryGetValue(productVariant.ProductId, out var modifierGroups)
                        ? modifierGroups.SelectMany(pmg => pmg.ModifierGroup.ModifierOptions)
                            .FirstOrDefault(x => x.Id == modifierOptionRequest.ModifierOptionId)
                        : null;

                    if (modifierOption == null)
                    {
                        return new ApiResponse()
                        {
                            Status = 400,
                            Message = "",
                        };
                    }

                    productForOrderResponse.ModifierOptions.Add(new ModifierOptionForOrderResponse()
                    {
                        Id = modifierOption.Id.ToString(),
                        ModifierGroupId = modifierOption.ModifierGroup.Id.ToString(),
                        ModifierGroupName = modifierOption.ModifierGroup.Name ?? string.Empty,
                        ModifierOptionName = modifierOption.Name ?? string.Empty,
                        DeltaPrice = (float)modifierOption.PriceDelta,
                        RelatedComboProductVariantItemId = string.Empty,
                        RelatedComboProductVariantItemName = string.Empty
                    });
                }
                else 
                {
                    if (!modifierOptionRequest.RelatedComboProductVariantItemId.HasValue)
                    {
                        return new ApiResponse()
                        {
                            Status = 400,
                            Message = "",
                        };
                    }

                    var comboItemVariantId = modifierOptionRequest.RelatedComboProductVariantItemId.Value;
                    var existingProductComboItem = comboItemsLookup.TryGetValue(productVariant.ProductId, out var comboList)
                        ? comboList.FirstOrDefault(x => x.ItemProductVariantId == comboItemVariantId)
                        : null;

                    if (existingProductComboItem == null)
                    {
                        return new ApiResponse()
                        {
                            Status = 400,
                            Message = "",
                        };
                    }

                    var existingModifierOption = comboModifierOptionsLookup.TryGetValue(modifierOptionRequest.ModifierOptionId, out var modifierOption)
                        ? modifierOption
                        : null;

                    if (existingModifierOption == null)
                    {
                        return new ApiResponse()
                        {
                            Status = 400,
                            Message = "",
                        };
                    }

                    var comboItemVariant = comboItemVariantsLookup.TryGetValue(comboItemVariantId, out var variant) ? variant : null;

                    productForOrderResponse.ModifierOptions.Add(new ModifierOptionForOrderResponse()
                    {
                        Id = existingModifierOption.Id.ToString(),
                        ModifierGroupId = existingModifierOption.ModifierGroup.Id.ToString(),
                        ModifierGroupName = existingModifierOption.ModifierGroup.Name ?? string.Empty,
                        ModifierOptionName = existingModifierOption.Name ?? string.Empty,
                        DeltaPrice = (float)existingModifierOption.PriceDelta,
                        RelatedComboProductVariantItemId = comboItemVariantId.ToString(),
                        RelatedComboProductVariantItemName = comboItemVariant?.Name ?? string.Empty
                    });
                }
            }
            response.ProductForOrders.Add(productForOrderResponse);
        }
    
        response.IsSuccess = true;
        response.ErrorMessage = string.Empty;
        return new ApiResponse()
        {
            Status = 200,
            Message = "",
            Data = response
        };
    }
}