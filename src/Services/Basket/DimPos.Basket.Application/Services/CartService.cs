using System.Text.Json;
using DimPos.Basket.Application.Common.Protos;
using DimPos.Basket.Application.Enums;
using DimPos.Basket.Application.Models;
using DimPos.Basket.Application.Models.Base;
using DimPos.Basket.Application.Models.ConditionRuleModel;
using DimPos.Basket.Application.Models.Request;
using DimPos.Basket.Application.Models.Response;
using DimPos.Basket.Application.Services.Interface;
using CartItemResponse = DimPos.Basket.Application.Models.Response.CartItemResponse;

namespace DimPos.Basket.Application.Services;

public class CartService : ICartService
{
    private readonly IRedisService _redisService;
    private readonly IClaimService _claimService;
    public CartService(IRedisService redisService, IClaimService claimService)
    {
        _redisService = redisService;
        _claimService = claimService;
    }

    public async Task<ApiResponse> AddToCartAsync(Guid cartId, AddToCartRequest request)
    {
        try
        {
            var staffAccountId = _claimService.GetCurrentUserId;
            if (staffAccountId == Guid.Empty)
            {
                throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
            }
            var cartKey = $"cart:{staffAccountId}"; 
            var existingCartJson = await _redisService.GetHashAsync(cartKey, cartId.ToString()); 
            if (string.IsNullOrEmpty(existingCartJson)) 
            { 
                throw new BadHttpRequestException("Không tìm thấy giỏ hàng"); 
            } 
            var cart = JsonSerializer.Deserialize<Cart>(existingCartJson); 
            var cartItem = new CartItem() 
            { 
                Id = Guid.CreateVersion7(),
                CartId = cartId, 
                ProductVariantId = request.ProductVariantId, 
                ProductNameSnapshot = request.ProductNameSnapshot, 
                ProductVariantNameSnapshot = request.ProductVariantNameSnapshot, 
                NotesForItem = request.NotesForItem, 
                UnitPriceAtAdditionSnapshot = request.UnitPriceAtAdditionSnapshot, 
                Quantity = request.Quantity, 
                ItemSubtotalAmount = request.UnitPriceAtAdditionSnapshot * request.Quantity, 
                AddedAt = DateTime.UtcNow,
                ModifierGroupItems = request.ModifierGroupItems?.Select(x => new ModifierGroupItem() 
                { 
                    ModifierGroupId = x.ModifierGroupId, 
                    ModifierOptionId = x.ModifierOptionId, 
                    ModifierGroupNameSnapshot = x.ModifierGroupNameSnapshot, 
                    ModifierOptionSnapshot = x.ModifierOptionSnapshot 
                }).ToList() 
            }; 
            var cartItemHashKey = $"{cartKey}:items"; 
            var cartItemSortedSetKey = $"{cartKey}:items:sortedset";
            var cartItemJson = JsonSerializer.Serialize(cartItem); 
            await _redisService.SetHashAsync(cartItemHashKey, cartItem.Id.ToString(), cartItemJson);
            await _redisService.SetSortedSetAsync(cartItemSortedSetKey, cartItem.Id.ToString(), cartItem.AddedAt.Ticks);
            
            if (cart != null) 
            { 
                cart.SubtotalAmount += cartItem.ItemSubtotalAmount; 
                cart.ItemCount += 1; 
                cart.TotalQuantityOfItems += request.Quantity; 
                cart.UpdatedAt = DateTime.UtcNow; 
                var updatedCartJson = JsonSerializer.Serialize(cart); 
                await _redisService.SetHashAsync(cartKey, cartId.ToString(), updatedCartJson); 
            } 
            return new ApiResponse 
            { 
                Status = StatusCodes.Status200OK, 
                Message = "Thêm sản phẩm vào giỏ hàng thành công", 
            };
        }
        catch (Exception e)
        {
            throw new Exception("Lỗi khi thêm sản phẩm vào giỏ hàng", e);
        }
    }

    public async Task<ApiResponse> CreateNewCartAsync(CreateNewCartRequest request)
    {
            var staffAccountId = _claimService.GetCurrentUserId;
            if (staffAccountId == Guid.Empty)
            {
                throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
            }
            var cartId = Guid.CreateVersion7();
            var hashkey = $"cart:{staffAccountId}";
            var sortedSetKey = $"{hashkey}:sortedset";
            var sortedSetExists = await _redisService.GetSortedSetAsync(sortedSetKey);
            if (sortedSetExists.Count >= 3)
            {
                throw new BadHttpRequestException("Giỏ hàng đã đạt giới hạn tối đa (3 giỏ hàng)");
            }
            var cart = new Cart()
            {
                Id = cartId,
                BrandId = request.BrandId,
                StoreId = request.StoreId,
                StaffAccountIdCreating = staffAccountId,
                CreatedAt = DateTime.UtcNow,
                ServiceMethod = EServiceMethod.DINE_IN,
                Status = ECartStatus.Active,
                ExpireAt = DateTime.UtcNow.AddHours(24),
                TakeNumberDineIn = 1, // Default take number for dine-in
                TaxRate = request.TaxRate
            };
            var cartJson = JsonSerializer.Serialize(cart);
            try
            {
                await _redisService.SetHashAsync(hashkey, cart.Id.ToString(), cartJson);
                await _redisService.SetSortedSetAsync(sortedSetKey, cart.Id.ToString(), cart.CreatedAt.Ticks);
                return new ApiResponse()
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Tạo giỏ hàng mới thành công",
                };
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi tạo giỏ hàng mới", e);
            }
    }

    public async Task<ApiResponse> DeleteCartAsync(Guid cartId)
    {
        var staffAccountId = _claimService.GetCurrentUserId;
        if (staffAccountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }
        var key = $"cart:{staffAccountId}";
        var carts = await _redisService.GetListAsync(key);
        if (carts == null || !carts.Any())
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }

        foreach (var cart in carts)
        {
            var cartJson = JsonSerializer.Deserialize<Cart>(cart);
            if (cartJson != null && cartJson.Id == cartId)
            {
                await _redisService.RemoveFromListAsync(key, cart);
                return new ApiResponse()
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Xoá giỏ hàng thành công",
                };
            }
        }
        throw new BadHttpRequestException("Không tìm thấy giỏ hàng để xoá");
    }

    public async Task<ApiResponse> GetCartAsync()
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }
        var hashCartKey = $"cart:{accountId}";
        var sortedSetCartKey = $"cart:{accountId}:sortedset";
        var cartIds = await _redisService.GetSortedSetAsync(sortedSetCartKey);
        
        var response = new List<CartResponse>();
        
        foreach (var cartId in cartIds)
        {
            var cartJson = await _redisService.GetHashAsync(hashCartKey, cartId);
            if (!string.IsNullOrEmpty(cartJson))
            {
                var cart = JsonSerializer.Deserialize<CartResponse>(cartJson);
                if (cart != null)
                {
                    var hashCartItemKey = $"{hashCartKey}:items";
                    var sortedSetCartItemKey = $"{hashCartKey}:items:sortedset";
                    var cartItemIds = await _redisService.GetSortedSetAsync(sortedSetCartItemKey);
                    foreach (var cartItemId in cartItemIds)
                    {
                        var cartItemJson = await _redisService.GetHashAsync(hashCartItemKey, cartItemId);
                        if (!string.IsNullOrEmpty(cartItemJson))
                        {
                            var cartItem = JsonSerializer.Deserialize<CartItemResponse>(cartItemJson);
                            if (cartItem != null)
                            {
                                cart.CartItems?.Add(cartItem);
                            }
                        }
                    }
                    var promotionSortedSetKey = $"{hashCartKey}:promotion:sortedset";
                    var promotionHashKey = $"{hashCartKey}:promotion";
                    var promotionIds = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
                    foreach (var promotionId in promotionIds)
                    {
                        var promotionJson = await _redisService.GetHashAsync(promotionHashKey, promotionId);
                        if (!string.IsNullOrEmpty(promotionJson))
                        {
                            var promotionDetail = JsonSerializer.Deserialize<PromotionResponse>(promotionJson);
                            if (promotionDetail != null)
                            {
                                cart.PromotionsApplied?.Add(promotionDetail);
                            }
                        }
                    }
                    response.Add(cart);
                }
            }
        }
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy giỏ hàng thành công",
            Data = response
        };
    }

    public async Task<ApiResponse> ApplePromotionAsync(Guid cartId, ApplyPromotionRequest request)
    {
        var staffAccountId = _claimService.GetCurrentUserId;
        if (staffAccountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }

        var cartAppliedPromotionDetail = new CartAppliedPromotionDetail()
        {
            Id = Guid.CreateVersion7(),
            CartId = cartId,
            PromotionRuleId = request.PromotionRuleId,
            PromotionNameSnapshot = request.PromotionNameSnapshot,
            ApplicableCartItemIds = request.ApplicableCartItemIds,
            CreatedAt = DateTime.UtcNow,
            ConditionRules = request.ConditionRules.Select(x => new ConditionRule()
            {
                ConditionType = x.ConditionType,
                Operator = x.Operator,
                ConditionValue = x.ConditionValue
            }).ToList(),
            ActionType = request.ActionType,
            ActionValue = request.ActionValue,
            MaxDiscountAmountForPercentage = request.MaxDiscountAmountForPercentage
        };
        var cartHashKey = $"cart:{staffAccountId}";
        var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        if(string.IsNullOrEmpty(cartJson))
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        var cartItemHashKey = $"{cartHashKey}:items";
        var cartItemSortedSetKey = $"{cartHashKey}:items:sortedset";
        var cartItemSortedSetExists = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
        var cartItemList = new List<CartItem>();
        var promotionSortedSetKey = $"{cartHashKey}:promotion:sortedset";
        var promotionHashKey = $"{cartHashKey}:promotion";
        foreach (var cartItemId in cartItemSortedSetExists)
        {
            var cartItemJson = await _redisService.GetHashAsync(cartItemHashKey, cartItemId);
            if (!string.IsNullOrEmpty(cartItemJson))
            {
                var cartItem = JsonSerializer.Deserialize<CartItem>(cartItemJson);
                if (cartItem != null)
                {
                    cartItemList.Add(cartItem);
                }
            }
        }
        foreach (var conditionRule in request.ConditionRules)
        {
            switch (conditionRule.ConditionType) 
            {
            case EConditionType.MinCartValue:
                var conditionValueDecimal = Decimal.Parse(conditionRule.ConditionValue);
                if (conditionRule.Operator == EOperator.GreaterThanOrEqual)
                {
                    if(cart.SubtotalAmount < conditionValueDecimal) 
                        throw new BadHttpRequestException("Giá trị giỏ hàng không đủ để áp dụng khuyến mãi");
                }
                else if (conditionRule.Operator == EOperator.GreaterThan)
                {
                    if(cart.SubtotalAmount <= conditionValueDecimal) 
                        throw new BadHttpRequestException("Giá trị giỏ hàng không đủ để áp dụng khuyến mãi");
                }
                else
                {
                    throw new BadHttpRequestException("Toán tử so sánh không hợp lệ");
                }
                break;
            case EConditionType.CartContainsProductVariant:
                List<Guid>? productVariantIds = JsonSerializer.Deserialize<List<Guid>>(conditionRule.ConditionValue);
                if(productVariantIds == null) 
                    throw new BadHttpRequestException("Không tìm thấy danh sách sản phẩm để áp dụng khuyến mãi");
                if (conditionRule.Operator == EOperator.ContainsAnyInList)
                {
                    if (!cartItemList.Any(ci => productVariantIds.Contains(ci.ProductVariantId)))
                    {
                        throw new BadHttpRequestException("Giỏ hàng không chứa sản phẩm nào trong danh sách để áp dụng khuyến mãi");
                    }
                }
                else if (conditionRule.Operator == EOperator.ContainsAllInList)
                {
                    if (!productVariantIds.All(pvId => cartItemList.Any(ci => ci.ProductVariantId == pvId)))
                    {
                        throw new BadHttpRequestException("Giỏ hàng không chứa tất cả sản phẩm trong danh sách để áp dụng khuyến mãi");
                    }
                }
                else if(conditionRule.Operator == EOperator.ContainsExactList)
                {
                    if (cartItemList.Count != productVariantIds.Count || 
                        !productVariantIds.All(pvId => cartItemList.Any(ci => ci.ProductVariantId == pvId)))
                    {
                        throw new BadHttpRequestException("Giỏ hàng không chứa đúng danh sách sản phẩm để áp dụng khuyến mãi");
                    }
                }
                else
                {
                    throw new BadHttpRequestException("Toán tử so sánh không hợp lệ");
                }
                break;
            case EConditionType.QuantityOfSpecificProductVariant:
                var quantityOfSpecificProductVariantModel = 
                    JsonSerializer.Deserialize<QuantityOfSpecificProductVariantModel>(conditionRule.ConditionValue);
                if (conditionRule.Operator == EOperator.GreaterThanOrEqual)
                {
                    if(!cartItemList.Any(ci => ci.ProductVariantId == quantityOfSpecificProductVariantModel.ProductVariantId 
                                               && ci.Quantity >= quantityOfSpecificProductVariantModel.Quantity))
                    {
                        throw new BadHttpRequestException("Số lượng sản phẩm không đủ để áp dụng khuyến mãi");
                    }
                }
                else if (conditionRule.Operator == EOperator.Equals)
                {
                    if (!cartItemList.Any(ci =>
                            ci.ProductVariantId == quantityOfSpecificProductVariantModel.ProductVariantId
                            && ci.Quantity == quantityOfSpecificProductVariantModel.Quantity))
                    {
                        throw new BadHttpRequestException("Số lượng sản phẩm không đủ để áp dụng khuyến mãi");
                    }
                }
                else
                {
                    throw new BadHttpRequestException("Toán tử so sánh không hợp lệ");
                }
                break;
            default: 
                throw new BadHttpRequestException("Kiểu điều kiện không hợp lệ");
            }
        }

        switch (request.ActionType)
        {
            case EActionType.CartPercentageDiscount:
                var percentageDiscount = Decimal.Parse(request.ActionValue);
                if (percentageDiscount < 0 || percentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị giảm giá phần trăm không hợp lệ");
                }
                cartAppliedPromotionDetail.DiscountValueCalculated = cart.SubtotalAmount * (percentageDiscount / 100);
                if(cartAppliedPromotionDetail.DiscountValueCalculated > request.MaxDiscountAmountForPercentage)
                {
                    cartAppliedPromotionDetail.DiscountValueCalculated = request.MaxDiscountAmountForPercentage;
                }
                cart.OrderLevelDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                
                break;
            case EActionType.CartFixedDiscount:
                var fixedDiscount = Decimal.Parse(request.ActionValue);
                if (fixedDiscount < 0)
                    throw new BadHttpRequestException("Giá trị giảm giá cố định không hợp lệ");

                cartAppliedPromotionDetail.DiscountValueCalculated = fixedDiscount;
                
                cart.OrderLevelDiscountAmount += fixedDiscount;
                
                break;
            case EActionType.ItemPercentageDiscount: 
                var percentageItemDiscount = Decimal.Parse(request.ActionValue);
                if(percentageItemDiscount < 0 || percentageItemDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị giảm giá phần trăm cho sản phẩm không hợp lệ");
                }

                if (request.TargetCriteriaForItemAction == null)
                {
                    break;
                }

                var cartItemForActionList = cartItemList
                    .Where(ci => request.TargetCriteriaForItemAction.Contains(ci.Id))
                    .ToList();
                foreach (var cartItemForAction in cartItemForActionList)
                {
                    var itemDiscountAmount = cartItemForAction.ItemSubtotalAmount * (percentageItemDiscount / 100);
                    cartAppliedPromotionDetail.DiscountValueCalculated += itemDiscountAmount * cartItemForAction.Quantity;
                }
                cart.TotalItemDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                break;
            case EActionType.OneItemPercentageDiscount: 
                var percentageFixedItemDiscount = Decimal.Parse(request.ActionValue);
                if(percentageFixedItemDiscount > 0 || percentageFixedItemDiscount < 100)
                {
                    throw new BadHttpRequestException("Giá trị giảm giá phần trăm cố định cho sản phẩm không hợp lệ");
                }
                if (request.TargetCriteriaForItemAction == null || request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Chỉ có thể áp dụng giảm giá phần trăm cố định cho một sản phẩm");
                }
                
                var targetCartItem = cartItemList
                    .FirstOrDefault(ci => ci.Id == request.TargetCriteriaForItemAction.FirstOrDefault());
                var itemDiscount = targetCartItem.ItemSubtotalAmount * (percentageFixedItemDiscount / 100);
                cartAppliedPromotionDetail.DiscountValueCalculated = itemDiscount;
                cart.TotalItemDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                break;
            case EActionType.ItemFixedAmountDiscount:
                var amountDiscount = Decimal.Parse(request.ActionValue);
                if (amountDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị giảm giá cố định cho sản phẩm không hợp lệ");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    break;
                }
                var cartItemForFixedActionList = cartItemList
                    .Where(ci => request.TargetCriteriaForItemAction.Contains(ci.Id))
                    .ToList();
                foreach (var cartItemForFixedAction in cartItemForFixedActionList)
                {
                    var cartItemForFixedActionFixed = amountDiscount;
                    cartAppliedPromotionDetail.DiscountValueCalculated += cartItemForFixedActionFixed * cartItemForFixedAction.Quantity;
                }
                cart.TotalItemDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                break;
            case EActionType.OneItemFixedAmountDiscount:
                var amountDiscountForOne = Decimal.Parse(request.ActionValue);
                if(amountDiscountForOne < 0)
                {
                    throw new BadHttpRequestException("Giá trị giảm giá cố định cho một sản phẩm không hợp lệ");
                }
                if(request.TargetCriteriaForItemAction == null)
                {
                    break;
                }
                if(request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Chỉ có thể áp dụng giảm giá cố định cho một sản phẩm");
                }
                var targetCartItemForFixed = cartItemList
                    .FirstOrDefault(ci => ci.ProductVariantId == request.TargetCriteriaForItemAction.FirstOrDefault());
                if (targetCartItemForFixed == null)
                {
                    throw new BadHttpRequestException("Không tìm thấy sản phẩm để áp dụng giảm giá cố định");
                }
                cartAppliedPromotionDetail.DiscountValueCalculated = amountDiscountForOne;
                cart.TotalItemDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                break;
            case EActionType.GiveFreeItemSku:
                var quantityFree = int.Parse(request.ActionValue);
                if (quantityFree < 0)
                {
                    throw new BadHttpRequestException("Số lượng sản phẩm miễn phí không hợp lệ");
                }
                if(request.TargetCriteriaForItemAction == null || request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Chỉ có thể áp dụng sản phẩm miễn phí cho một sản phẩm");
                }
                var targetCartItemForFreeList = cartItemList
                    .Where(ci => request.TargetCriteriaForItemAction.Contains(ci.Id)).ToList();
                foreach (var targetCartItemForFree in targetCartItemForFreeList)
                {
                    targetCartItemForFree.Quantity += quantityFree;
                    cartAppliedPromotionDetail.DiscountValueCalculated += targetCartItemForFree.UnitPriceAtAdditionSnapshot * quantityFree;
                    await _redisService.SetHashAsync(cartItemHashKey, targetCartItemForFree.Id.ToString(), JsonSerializer.Serialize(targetCartItemForFree));
                }
                cart.TotalItemDiscountAmount += cartAppliedPromotionDetail.DiscountValueCalculated;
                break;
            default: 
                throw new BadHttpRequestException("Kiểu hành động không hợp lệ");
        }
        if (cart.TaxRate != null)
        {
            cart.TotalTaxAmount = (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount) * (cart.TaxRate.Value / 100);
        }
        cart.FinalTotalAmount = cart.TotalTaxAmount +
                                (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount);
        var cartAppliedPromotionDetailJson = JsonSerializer.Serialize(cartAppliedPromotionDetail);
        await _redisService.SetHashAsync(promotionHashKey, cartAppliedPromotionDetail.Id.ToString(), cartAppliedPromotionDetailJson);
        await _redisService.SetSortedSetAsync(promotionSortedSetKey, cartAppliedPromotionDetail.Id.ToString(), cartAppliedPromotionDetail.CreatedAt.Ticks);
        var updatedCartJson = JsonSerializer.Serialize(cart);
        await _redisService.SetHashAsync(cartHashKey, cart.Id.ToString(), updatedCartJson);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Áp dụng khuyến mãi thành công",
            Data = null
        };
    }
}