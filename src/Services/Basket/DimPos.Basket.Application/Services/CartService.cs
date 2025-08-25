using System.Text.Json;
using DimPos.Basket.Application.Common.Protos;
using DimPos.Basket.Application.Common.Utils;
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
    #region Redis Keys
    private static string GetCartHashKey(Guid staffId) => $"cart:{staffId.ToString()}";
    private static string GetCartSortedSetKey(Guid staffId)
        => $"cart:{staffId.ToString()}:sortedset";
    private static string GetCartItemsHashKey(Guid staffId, Guid cartId)
        => $"cart:{staffId.ToString()}:items:{cartId.ToString()}";
    private static string GetCartItemsSortedSetKey(Guid staffId, Guid cartId)
        => $"cart:{staffId.ToString()}:items:{cartId.ToString()}:sortedset";
    private static string GetCartPromotionHashKey(Guid staffId, Guid cartId)
        => $"cart:{staffId.ToString()}:promotion:{cartId.ToString()}";
    private static string GetCartPromotionSortedSetKey(Guid staffId, Guid cartId)
        => $"cart:{staffId.ToString()}:promotion:{cartId.ToString()}:sortedset";
    #endregion
    public async Task<ApiResponse> AddToCartAsync(Guid cartId, AddToCartRequest request)
    {
        try
        {
            var staffAccountId = _claimService.GetCurrentUserId;
            if (staffAccountId == Guid.Empty)
            {
                throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
            }
            var cartKey = GetCartHashKey(staffAccountId); 
            var existingCartJson = await _redisService.GetHashAsync(cartKey, cartId.ToString()); 
            if (string.IsNullOrEmpty(existingCartJson)) 
            { 
                throw new BadHttpRequestException("Không tìm thấy giỏ hàng"); 
            } 
            var cart = JsonSerializer.Deserialize<Cart>(existingCartJson); 
            var cartItemHashKey = GetCartItemsHashKey(staffAccountId, cartId);
            var cartItemSortedSetKey = GetCartItemsSortedSetKey(staffAccountId, cartId);
            var cartItemIdSortedSetExists = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
            foreach (var cartItemIdExisting in cartItemIdSortedSetExists)
            {
                var existingCartItemJson = await _redisService.GetHashAsync(cartItemHashKey, cartItemIdExisting);
                var existingCartItem = JsonSerializer.Deserialize<CartItem>(existingCartItemJson);
                if (existingCartItem != null)
                {
                    var firstIds  = existingCartItem.ModifierGroupItems.Select(x => x.ModifierOptionId).OrderBy(id => id);
                    var secondIds = request.ModifierGroupItems.Select(x => x.ModifierOptionId).OrderBy(id => id);
                    
                    var firstExtraIds = existingCartItem.ExtraItems?.Select(x => x.ExtraProductVariantId).OrderBy(id => id) ?? Enumerable.Empty<Guid>();
                    var secondExtraIds = request.ExtraItems?.Select(x => x.ExtraProductVariantId).OrderBy(id => id) ?? Enumerable.Empty<Guid>();
                    
                    if (existingCartItem.ProductVariantId == request.ProductVariantId
                        && firstIds.SequenceEqual(secondIds) && firstExtraIds.SequenceEqual(secondExtraIds))
                    {
                        throw new BadHttpRequestException("Sản phẩm đã tồn tại trong giỏ hàng với cùng biến thể và tùy chọn sửa đổi");
                    }
                }
            }
            var cartItem = new CartItem() 
            { 
                Id = Guid.CreateVersion7(),
                CartId = cartId, 
                ProductVariantId = request.ProductVariantId, 
                ProductNameSnapshot = request.ProductNameSnapshot, 
                ProductVariantNameSnapshot = request.ProductVariantNameSnapshot, 
                ProductImageUrlSnapshot = request.ProductImageUrlSnapshot,
                NotesForItem = request.NotesForItem, 
                UnitPriceAtAdditionSnapshot = request.UnitPriceAtAdditionSnapshot, 
                Quantity = request.Quantity, 
                ItemSubtotalAmount = (request.UnitPriceAtAdditionSnapshot) * request.Quantity,
                TotalPriceOfProductExtraItems = 0,
                AddedAt = TimeUtil.GetCurrentSEATime(),
                ModifierGroupItems = request.ModifierGroupItems?.Select(x => new ModifierGroupItem() 
                { 
                    ModifierGroupId = x.ModifierGroupId, 
                    ModifierOptionId = x.ModifierOptionId, 
                    ModifierGroupNameSnapshot = x.ModifierGroupNameSnapshot, 
                    ModifierOptionSnapshot = x.ModifierOptionSnapshot,
                    RelatedComboProductVariantItemId = x.RelatedComboProductVariantItemId,
                    RelatedComboProductVariantItemName = x.RelatedComboProductVariantItemName
                }).ToList(),
                ExtraItems = request.ExtraItems?.Select(x => new ProductExtraItem() 
                { 
                    ExtraProductVariantId = x.ExtraProductVariantId, 
                    ExtraProductVariantNameSnapshot = x.ExtraProductVariantNameSnapshot, 
                    Quantity = x.Quantity, 
                    UnitPriceAtAdditionSnapshot = x.UnitPriceAtAdditionSnapshot, 
                    RelatedProductVariantId = x.RelatedProductVariantId
                }).ToList()
            }; 
            if(request.ExtraItems != null && request.ExtraItems.Any())
            {
                cartItem.TotalPriceOfProductExtraItems =  request.ExtraItems.Sum(x => x.UnitPriceAtAdditionSnapshot * x.Quantity);
                cartItem.ItemSubtotalAmount += cartItem.TotalPriceOfProductExtraItems;
            }
            var cartItemJson = JsonSerializer.Serialize(cartItem); 
            
            var promotionSortedSetKey = GetCartPromotionSortedSetKey(staffAccountId, cartId);
            var promotionHashKey = GetCartPromotionHashKey(staffAccountId, cartId);
            
            var promotionSortedSetExists = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
            if (promotionSortedSetExists.Any())
            {
                foreach (var promotionId in promotionSortedSetExists)
                {
                    var promotionJson = await _redisService.GetHashAsync(promotionHashKey, promotionId);
                    if (!string.IsNullOrEmpty(promotionJson))
                    {
                        var existingPromotion = JsonSerializer.Deserialize<CartAppliedPromotionDetail>(promotionJson);
                        switch (existingPromotion.ActionType)
                        {
                            case EActionType.CartPercentageDiscount:
                                var existingDiscountValueCalculated = existingPromotion.DiscountValueCalculated;
                                var percentageDiscount = Decimal.Parse(existingPromotion.ActionValue);
                                if (percentageDiscount < 0 || percentageDiscount > 100)
                                {
                                    throw new BadHttpRequestException("Giá trị giảm giá phần trăm không hợp lệ");
                                }

                                cart.SubtotalAmount += cartItem.ItemSubtotalAmount;
                                existingPromotion.DiscountValueCalculated = cart.SubtotalAmount * (percentageDiscount / 100);
                                if (existingPromotion.MaxDiscountAmountForPercentage != null &&
                                    existingPromotion.MaxDiscountAmountForPercentage > 0 &&
                                    existingPromotion.DiscountValueCalculated > existingPromotion.MaxDiscountAmountForPercentage)
                                {
                                    existingPromotion.DiscountValueCalculated = existingPromotion.MaxDiscountAmountForPercentage.Value;
                                }
                                cart.OrderLevelDiscountAmount += existingPromotion.DiscountValueCalculated - existingDiscountValueCalculated;
                                break;
                            case EActionType.ItemFixedAmountDiscount:
                                var amountDiscount = Decimal.Parse(existingPromotion.ActionValue);
                                if (amountDiscount < 0)
                                {
                                    throw new BadHttpRequestException("Giá trị giảm giá cố định cho sản phẩm không hợp lệ");
                                }
                                if (existingPromotion.TargetCriteriaForItemAction == null)
                                {
                                    break;
                                }

                                if (existingPromotion.TargetCriteriaForItemAction != null &&
                                    existingPromotion.TargetCriteriaForItemAction.Contains(cartItem.Id))
                                {
                                    existingPromotion.DiscountValueCalculated += amountDiscount * cartItem.Quantity;
                                    cart.TotalItemDiscountAmount += existingPromotion.DiscountValueCalculated;
                                }
                                break;
                            case EActionType.ItemPercentageDiscount:
                                var existingItemDiscountValueCalculated = existingPromotion.DiscountValueCalculated;
                                var percentageItemDiscount = Decimal.Parse(existingPromotion.ActionValue);
                                if (percentageItemDiscount < 0 || percentageItemDiscount > 100)
                                {
                                    throw new BadHttpRequestException("Giá trị giảm giá phần trăm cho sản phẩm không hợp lệ");
                                }
                                if(existingPromotion.TargetCriteriaForItemAction != null &&
                                   existingPromotion.TargetCriteriaForItemAction.Contains(cartItem.Id))
                                {
                                    var itemDiscountAmount = cartItem.ItemSubtotalAmount * (percentageItemDiscount / 100);
                                    existingPromotion.DiscountValueCalculated += itemDiscountAmount * cartItem.Quantity;
                                    cart.TotalItemDiscountAmount += existingPromotion.DiscountValueCalculated - existingItemDiscountValueCalculated;
                                }
                                break;
                            case EActionType.OneItemPercentageDiscount:
                            case EActionType.OneItemFixedAmountDiscount:
                            case EActionType.GiveFreeItemSku:
                            case EActionType.CartFixedDiscount:
                                cart.SubtotalAmount += cartItem.ItemSubtotalAmount;
                                break;
                            default:
                                throw new BadHttpRequestException("Kiểu hành động không hợp lệ");
                        }
                        await _redisService.SetHashAsync(promotionHashKey, promotionId, JsonSerializer.Serialize(existingPromotion));
                    }
                }
            }
            else
            {
                cart.SubtotalAmount += cartItem.ItemSubtotalAmount;
            }
            await _redisService.SetHashAsync(cartItemHashKey, cartItem.Id.ToString(), cartItemJson);
            await _redisService.SetSortedSetAsync(cartItemSortedSetKey, cartItem.Id.ToString(), cartItem.AddedAt.Ticks);
            
            cart.ItemCount += 1; 
            cart.TotalQuantityOfItems += request.Quantity; 
            cart.UpdatedAt = TimeUtil.GetCurrentSEATime();
            if (cart.TaxRate != null)
            {
                cart.TotalTaxAmount = (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount) * (cart.TaxRate.Value / 100);
            }
            cart.FinalTotalAmount = cart.TotalTaxAmount +
                                    (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount);
            var updatedCartJson = JsonSerializer.Serialize(cart); 
            await _redisService.SetHashAsync(cartKey, cartId.ToString(), updatedCartJson); 
            
            return new ApiResponse 
            { 
                Status = StatusCodes.Status200OK, 
                Message = "Thêm sản phẩm vào giỏ hàng thành công", 
            };
        }
        catch (Exception e)
        {
            // throw new Exception("Lỗi khi thêm sản phẩm vào giỏ hàng", e);
            throw new Exception(e.Message);

        }
    }

    public async Task<ApiResponse> CreateNewCartAsync(CreateNewCartRequest request)
    {
            var staffAccountId = _claimService.GetCurrentUserId;
            if (staffAccountId == Guid.Empty)
            {
                throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
            }

            var storeId = _claimService.GetStoreId ?? Guid.Empty;
            if (storeId == Guid.Empty)
            {
                throw new BadHttpRequestException("Không tìm thấy cửa hàng");
            }
            
            var cartId = Guid.CreateVersion7();
            var hashkey = GetCartHashKey(staffAccountId);
            var sortedSetKey = GetCartSortedSetKey(staffAccountId);
            var sortedSetExists = await _redisService.GetSortedSetAsync(sortedSetKey);
            if (sortedSetExists.Count >= 3)
            {
                throw new BadHttpRequestException("Giỏ hàng đã đạt giới hạn tối đa (3 giỏ hàng)");
            }
            var cart = new Cart()
            {
                Id = cartId,
                BrandId = request.BrandId,
                StoreId = storeId,
                StaffAccountIdCreating = staffAccountId,
                CreatedAt = TimeUtil.GetCurrentSEATime(),
                ServiceMethod = EServiceMethod.DINE_IN,
                Status = ECartStatus.Active,
                ExpireAt = TimeUtil.GetCurrentSEATime().AddHours(24),
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
        var hashCartKey = GetCartHashKey(staffAccountId);

        var cartJson = await _redisService.GetHashAsync(hashCartKey, cartId.ToString());
        if (string.IsNullOrEmpty(cartJson))
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        if (cart == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }

        var sortedSetCartKey = GetCartSortedSetKey(staffAccountId);
        var cartItemHashKey = GetCartItemsHashKey(staffAccountId, cart.Id);
        var cartItemSortedSetKey = GetCartItemsSortedSetKey(staffAccountId, cart.Id);
        var promotionSortedSetKey = GetCartPromotionSortedSetKey(staffAccountId, cart.Id);
        var promotionHashKey = GetCartPromotionHashKey(staffAccountId, cart.Id);
        
        var cartItemId = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
        if (cartItemId.Any())
        {
            foreach (var itemId in cartItemId)
            {
                await _redisService.RemoveHashAsync(cartItemHashKey, itemId);
                await _redisService.RemoveSortedSetAsync(cartItemSortedSetKey, itemId);
            }
        }
        var promotionIds = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
        if (promotionIds.Any())
        {
            foreach (var promotionId in promotionIds)
            {
                await _redisService.RemoveHashAsync(promotionHashKey, promotionId);
                await _redisService.RemoveSortedSetAsync(promotionSortedSetKey, promotionId);
            }
        }
        
        await _redisService.RemoveHashAsync(hashCartKey, cartId.ToString());
        await _redisService.RemoveSortedSetAsync(sortedSetCartKey, cartId.ToString());
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Xóa giỏ hàng thành công"
        };
    }

    public async Task<ApiResponse> GetCartAsync()
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }
        var hashCartKey = GetCartHashKey(accountId);
        var sortedSetCartKey = GetCartSortedSetKey(accountId);
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
                    var hashCartItemKey = GetCartItemsHashKey(accountId, cart.Id);
                    var sortedSetCartItemKey = GetCartItemsSortedSetKey(accountId, cart.Id);
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
                    var promotionSortedSetKey = GetCartPromotionSortedSetKey(accountId, cart.Id);
                    var promotionHashKey = GetCartPromotionHashKey(accountId, cart.Id);
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

    public async Task<ApiResponse> ApplyPromotionAsync(Guid cartId, ApplyPromotionRequest request)
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
            CreatedAt = TimeUtil.GetCurrentSEATime(),
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
        var cartHashKey = GetCartHashKey(staffAccountId);
        var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        if(string.IsNullOrEmpty(cartJson))
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        var cartItemHashKey = GetCartItemsHashKey(staffAccountId, cartId);
        var cartItemSortedSetKey = GetCartItemsSortedSetKey(staffAccountId, cartId);
        var cartItemSortedSetExists = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
        var cartItemList = new List<CartItem>();
        var promotionSortedSetKey = GetCartPromotionSortedSetKey(staffAccountId, cartId);
        var promotionHashKey = GetCartPromotionHashKey(staffAccountId, cartId);
        var promotionSortedSetExists = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
        if (promotionSortedSetExists.Any())
        {
            foreach (var promotionId in promotionSortedSetExists)
            {
                var promotionJson = await _redisService.GetHashAsync(promotionHashKey, promotionId);
                if (!string.IsNullOrEmpty(promotionJson))
                {
                    var existingPromotion = JsonSerializer.Deserialize<CartAppliedPromotionDetail>(promotionJson);
                    if (existingPromotion != null)
                    {
                        if (existingPromotion.PromotionRuleId.Equals(request.PromotionRuleId))
                        {
                            throw new BadHttpRequestException("Khuyến mãi đã được áp dụng cho giỏ hàng này");
                        }
                        if((request.ActionType == EActionType.CartFixedDiscount || request.ActionType == EActionType.CartPercentageDiscount)
                           && (existingPromotion.ActionType == EActionType.CartFixedDiscount ||
                               existingPromotion.ActionType == EActionType.CartPercentageDiscount))
                        {
                            throw new BadHttpRequestException("Giỏ hàng đã áp dụng khuyến mãi với loại hành động tương tự");
                        }
                    }
                }
            }
        }
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
                if(percentageFixedItemDiscount < 0 || percentageFixedItemDiscount > 100)
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
                var targetCartItemForFree = cartItemList
                    .FirstOrDefault(ci => request.TargetCriteriaForItemAction.Contains(ci.Id));
                if(targetCartItemForFree == null)
                {
                    throw new BadHttpRequestException("Không tìm thấy sản phẩm để áp dụng miễn phí");
                }
                targetCartItemForFree.Quantity += quantityFree;
                cartAppliedPromotionDetail.DiscountValueCalculated += targetCartItemForFree.UnitPriceAtAdditionSnapshot * quantityFree;
                await _redisService.SetHashAsync(cartItemHashKey, targetCartItemForFree.Id.ToString(), JsonSerializer.Serialize(targetCartItemForFree));
                
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

    public async Task<ApiResponse> UpdateCartAsync(Guid cartId, UpdateCartRequest request)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }

        var cartHashKey = GetCartHashKey(accountId);
        var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        if (string.IsNullOrEmpty(cartJson))
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        cart.TakeNumberDineIn = request.TakeNumberDineIn;
        cart.ServiceMethod = request.ServiceMethod ?? cart.ServiceMethod;
        
        await _redisService.SetHashAsync(cartHashKey, cart.Id.ToString(), JsonSerializer.Serialize(cart));
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật giỏ hàng thành công",
            Data = null
        };
    }

    public async Task<ApiResponse> UpdateCartItemAsync(Guid cartId, Guid cartItemId, UpdateCartItemRequest request)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }

        var cartHashKey = GetCartHashKey(accountId);
        var cartItemHashKey = GetCartItemsHashKey(accountId, cartId);
        var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        if (string.IsNullOrEmpty(cartJson))
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        var cartItemJson = await _redisService.GetHashAsync(cartItemHashKey, cartItemId.ToString());
        if (string.IsNullOrEmpty(cartItemJson))
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm trong giỏ hàng");
        }
        
        var cartItem = JsonSerializer.Deserialize<CartItem>(cartItemJson);
        if (cartItem == null)
        {
            throw new BadHttpRequestException("Không tìm thấy sản phẩm trong giỏ hàng");
        }
        cartItem.NotesForItem = string.IsNullOrEmpty(request.NotesForItem) 
            ? cartItem.NotesForItem 
            : request.NotesForItem;
        if (request.Quantity != null)
        {
            if (request.Quantity < 0)
            {
                throw new BadHttpRequestException("Số lượng sản phẩm không hợp lệ");
            }

            if (request.Quantity == 0)
            {
                cart.ItemCount -= 1;
                cart.TotalQuantityOfItems -= cartItem.Quantity;
                cart.SubtotalAmount -= cartItem.ItemSubtotalAmount;
                // Xoá sản phẩm khỏi giỏ hàng nếu số lượng là 0
                var cartItemSortedSetKey = GetCartItemsSortedSetKey(accountId, cartId);
                await _redisService.RemoveSortedSetAsync(cartItemSortedSetKey, cartItemId.ToString());
                await _redisService.RemoveHashAsync(cartItemHashKey, cartItemId.ToString());
            }
            else
            {
                var previousQuantity = cartItem.Quantity;
                cartItem.Quantity = request.Quantity.Value;
                cartItem.ItemSubtotalAmount = cartItem.UnitPriceAtAdditionSnapshot * cartItem.Quantity;
                cart.SubtotalAmount -= (previousQuantity * cartItem.UnitPriceAtAdditionSnapshot) 
                                        - (cartItem.Quantity * cartItem.UnitPriceAtAdditionSnapshot);
                cart.TotalQuantityOfItems += (cartItem.Quantity - previousQuantity);
                
                if (request.ModifierGroupItems != null && request.ModifierGroupItems.Any())
                {
                    var modifierGroupItems = request.ModifierGroupItems.Select(x => new ModifierGroupItem()
                    {
                        ModifierGroupId = x.ModifierGroupId,
                        ModifierOptionId = x.ModifierOptionId,
                        ModifierGroupNameSnapshot = x.ModifierGroupNameSnapshot,
                        ModifierOptionSnapshot = x.ModifierOptionSnapshot,
                        PriceDeltaSnapshot = x.PriceDeltaSnapshot,
                        RelatedComboProductVariantItemId = x.RelatedComboProductVariantItemId,
                        RelatedComboProductVariantItemName = x.RelatedComboProductVariantItemName
                    }).ToList();
                    cartItem.ModifierGroupItems = modifierGroupItems;
                    
                }

                if (request.ExtraItems != null && request.ExtraItems.Any())
                {
                    var extraItems = request.ExtraItems.Select(x => new ProductExtraItem()
                    {
                        ExtraProductVariantId = x.ExtraProductVariantId,
                        ExtraProductVariantNameSnapshot = x.ExtraProductVariantNameSnapshot,
                        Quantity = x.Quantity,
                        UnitPriceAtAdditionSnapshot = x.UnitPriceAtAdditionSnapshot,
                        RelatedProductVariantId = x.RelatedProductVariantId
                    }).ToList();
                    var previousTotalPriceOfProductExtraItems = cartItem.TotalPriceOfProductExtraItems;
                    cartItem.TotalPriceOfProductExtraItems = extraItems.Sum(x => x.UnitPriceAtAdditionSnapshot * x.Quantity);
                    cartItem.ItemSubtotalAmount += cartItem.TotalPriceOfProductExtraItems;
                    
                    cartItem.ExtraItems = extraItems;
                    cart.SubtotalAmount += cartItem.TotalPriceOfProductExtraItems - previousTotalPriceOfProductExtraItems;
                }
                var promotionSortedSetKey = GetCartPromotionSortedSetKey(accountId, cartId);
                var promotionHashKey = GetCartPromotionHashKey(accountId, cartId);
                var promotionIdSortedSetExists = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
                if (promotionIdSortedSetExists.Any())
                {
                    foreach (var promotionIdSortedSetExist in promotionIdSortedSetExists)
                    {
                        var promotionJson =
                            await _redisService.GetHashAsync(promotionHashKey, promotionIdSortedSetExist);
                        if (!string.IsNullOrEmpty(promotionJson))
                        {
                            var existingPromotion =
                                JsonSerializer.Deserialize<CartAppliedPromotionDetail>(promotionJson);
                            var existingPromotionDiscountValue = existingPromotion.DiscountValueCalculated;
                            switch (existingPromotion.ActionType)
                            {
                                case EActionType.CartPercentageDiscount:
                                    var percentageDiscount = Decimal.Parse(existingPromotion.ActionValue);
                                    if (percentageDiscount < 0 || percentageDiscount > 100)
                                    {
                                        throw new BadHttpRequestException("Giá trị giảm giá phần trăm không hợp lệ");
                                    }

                                    existingPromotion.DiscountValueCalculated = cart.SubtotalAmount * (percentageDiscount / 100);
                                    if (existingPromotion.MaxDiscountAmountForPercentage.HasValue &&
                                        existingPromotion.DiscountValueCalculated >
                                        existingPromotion.MaxDiscountAmountForPercentage.Value)
                                    {
                                        existingPromotion.DiscountValueCalculated =
                                            existingPromotion.MaxDiscountAmountForPercentage.Value;
                                    }
                                    cart.OrderLevelDiscountAmount += existingPromotion.DiscountValueCalculated - existingPromotionDiscountValue;
                                    break;
                                case EActionType.ItemPercentageDiscount:
                                    var itemPercentageAmount = Decimal.Parse(existingPromotion.ActionValue);
                                    if (itemPercentageAmount < 0 || itemPercentageAmount > 100)
                                    {
                                        throw new BadHttpRequestException("Giá trị giảm giá phần trăm cho sản phẩm không hợp lệ");
                                    }
                                    if (existingPromotion.TargetCriteriaForItemAction != null &&
                                        existingPromotion.TargetCriteriaForItemAction.Contains(cartItem.Id))
                                    {
                                        var itemDiscountAmount = cartItem.ItemSubtotalAmount * (itemPercentageAmount / 100);
                                        existingPromotion.DiscountValueCalculated = itemDiscountAmount * cartItem.Quantity;
                                        cart.TotalItemDiscountAmount += existingPromotion.DiscountValueCalculated - existingPromotionDiscountValue;
                                    }
                                    break;
                                case EActionType.ItemFixedAmountDiscount:
                                    var itemFixedAmount = Decimal.Parse(existingPromotion.ActionValue);
                                    if (itemFixedAmount < 0)
                                    {
                                        throw new BadHttpRequestException("Giá trị giảm giá cố định cho sản phẩm không hợp lệ");
                                    }
                                    if (existingPromotion.TargetCriteriaForItemAction != null &&
                                        existingPromotion.TargetCriteriaForItemAction.Contains(cartItem.Id))
                                    {
                                        existingPromotion.DiscountValueCalculated = itemFixedAmount * cartItem.Quantity;
                                        cart.TotalItemDiscountAmount += existingPromotion.DiscountValueCalculated - existingPromotionDiscountValue;
                                    }
                                    break;
                                case EActionType.CartFixedDiscount:
                                    break;
                                case EActionType.OneItemFixedAmountDiscount:
                                    break;
                                case EActionType.OneItemPercentageDiscount:
                                    break;
                                default:
                                    throw new BadHttpRequestException("Kiểu hành động không hợp lệ");
                            }
                            await _redisService.SetHashAsync(promotionHashKey, existingPromotion.Id.ToString(), JsonSerializer.Serialize(existingPromotion));
                        }
                    }
                }
                await _redisService.SetHashAsync(cartItemHashKey, cartItem.Id.ToString(), JsonSerializer.Serialize(cartItem));
                await _redisService.SetHashAsync(cartHashKey, cart.Id.ToString(), JsonSerializer.Serialize(cart));
            }
            if (cart.TaxRate != null)
            {
                cart.TotalTaxAmount = (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount) * (cart.TaxRate.Value / 100);
            }
            
            cart.FinalTotalAmount = cart.TotalTaxAmount +
                                    (cart.SubtotalAmount - cart.OrderLevelDiscountAmount - cart.TotalItemDiscountAmount);
            await _redisService.SetHashAsync(cartHashKey, cart.Id.ToString(), JsonSerializer.Serialize(cart));
        }
        
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật sản phẩm trong giỏ hàng thành công",
            Data = null
        };
    }

    public async Task<ApiResponse> RemoveAppliedPromotionAsync(Guid cartId, RemoveCartAppliedPromotionDetailRequest request)
    {
        var currentUserId = _claimService.GetCurrentUserId;
        if (currentUserId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        }
        
        var cartHashKey = GetCartHashKey(currentUserId);
        var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        if (string.IsNullOrEmpty(cartJson))
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }
        var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        if (cart == null)
        {
            throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        }
        
        var promotionSortedSetKey = GetCartPromotionSortedSetKey(currentUserId, cart.Id);
        var promotionHashKey = GetCartPromotionHashKey(currentUserId, cart.Id);
        var promotionIdSortedSetExists = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
        foreach (var promotionIdRequest in request.PromotionIds)
        {
            if (!promotionIdSortedSetExists.Contains(promotionIdRequest.ToString()))
            {
                throw new BadHttpRequestException("Không tìm thấy khuyến mãi để xoá");
            }
            var promotionJson = await _redisService.GetHashAsync(promotionHashKey, promotionIdRequest.ToString());
            if (string.IsNullOrEmpty(promotionJson))
            {
                throw new BadHttpRequestException("Không tìm thấy khuyến mãi để xoá");
            }
            var promotionDetail = JsonSerializer.Deserialize<CartAppliedPromotionDetail>(promotionJson);
            if (promotionDetail == null)
            {
                throw new BadHttpRequestException("Không tìm thấy khuyến mãi để xoá");
            }

            switch (promotionDetail.ActionType)
            {
                case EActionType.GiveFreeItemSku:
                    var cartItemHashKey = GetCartItemsHashKey(currentUserId, cart.Id);
                    var cartItemSortedSetKey = GetCartItemsSortedSetKey(currentUserId, cart.Id);
                    var cartItemSortedSetExists = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
                    var targetProductVariantId = promotionDetail.TargetCriteriaForItemAction.FirstOrDefault();
                    var targetQuantity = int.Parse(promotionDetail.ActionValue);
                    if (cartItemSortedSetExists.Any())
                    {
                        var cartItemList = new List<CartItem>();
                        foreach (var cartItemId in cartItemSortedSetExists)
                        {
                            var cartItem = await _redisService.GetHashAsync(cartItemHashKey, cartItemId);
                            if (!string.IsNullOrEmpty(cartItem))
                            {
                                cartItemList.Add(JsonSerializer.Deserialize<CartItem>(cartItem));
                            }
                        }

                        var targetCartItem =
                            cartItemList.FirstOrDefault(x => x.ProductVariantId.Equals(targetProductVariantId));
                        if (targetCartItem != null)
                        {
                            targetCartItem.Quantity -= targetQuantity;
                            cart.TotalQuantityOfItems -= targetQuantity;
                            cart.TotalItemDiscountAmount -= promotionDetail.DiscountValueCalculated;
                        }
                        await _redisService.SetHashAsync(cartItemHashKey, targetCartItem.Id.ToString(), JsonSerializer.Serialize(targetCartItem));
                    }
                    break;
                case EActionType.CartFixedDiscount:
                case EActionType.CartPercentageDiscount:
                    cart.OrderLevelDiscountAmount -= promotionDetail.DiscountValueCalculated;
                    break;
                case EActionType.ItemFixedAmountDiscount:
                case EActionType.ItemPercentageDiscount:
                case EActionType.OneItemFixedAmountDiscount:
                case EActionType.OneItemPercentageDiscount:
                    cart.TotalItemDiscountAmount -= promotionDetail.DiscountValueCalculated;
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
            
            await _redisService.RemoveHashAsync(promotionHashKey, promotionIdRequest.ToString());
            await _redisService.RemoveSortedSetAsync(promotionSortedSetKey, promotionIdRequest.ToString());
            
            await _redisService.SetHashAsync(cartHashKey, cart.Id.ToString(), JsonSerializer.Serialize(cart));
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xoá khuyến mãi thành công",
            Data = null
        };
    }
}