using System.Text.Json;
using DimPos.Basket.Application.Common.Protos;
using DimPos.Basket.Application.Models;
using DimPos.Basket.Application.Services.Interface;
using Grpc.Core;

namespace DimPos.Basket.Application.GrpcServices;

public class BasketGrpcService : Common.Protos.BasketGrpcService.BasketGrpcServiceBase
{
    private readonly IRedisService _redisService;
    public BasketGrpcService(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public override async Task<GetCartByCartIdResponse> GetCartByCartId(GetCartByCartIdRequest request, ServerCallContext context)
    {
        var staffAccountId = Guid.Parse(request.StaffAccountId);
        if (staffAccountId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id của nhân viên không hợp lệ.")); 
        }
        var cartId = Guid.Parse(request.CartId);
        if (cartId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id của giỏ hàng không hợp lệ.")); 
        }
        
        var cartHashKey = $"cart:{staffAccountId}";
        var cartFromRedis = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        
        if (cartFromRedis == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Giỏ hàng không tồn tại hoặc đã bị xóa."));
        }

        var cart = JsonSerializer.Deserialize<Cart>(cartFromRedis);
        
        var response = new GetCartByCartIdResponse();

        if (cart != null)
        {
            response.Id = cart.Id.ToString();
            response.StoreId = cart.StoreId.ToString() ?? string.Empty;
            response.BrandId = cart.BrandId.ToString() ?? string.Empty;
            response.PosDeviceId = cart.PosDeviceId.ToString() ?? string.Empty;
            response.StaffAccountIdCreating = cart.StaffAccountIdCreating.ToString() ?? String.Empty;
            response.CustomerIdLinked = cart.CustomerIdLinked.ToString() ?? String.Empty;
            response.TakeNumberDineIn = cart.TakeNumberDineIn ?? 0;
            response.SubtotalAmount = (float) cart.SubtotalAmount;
            response.TotalItemDiscountAmount = (float) cart.TotalItemDiscountAmount;
            response.OrderLevelDiscountAmount = (float) cart.OrderLevelDiscountAmount;
            response.TaxRate = (float) (cart.TaxRate ?? 0);
            response.TotalTaxAmount = (float) cart.TotalTaxAmount;
            response.FinalTotalAmount = (float) cart.FinalTotalAmount;
            response.ItemCount = cart.ItemCount;
            response.TotalQuantityOfItems = cart.TotalQuantityOfItems;
            
            var cartItemSortedSetKey = $"{cartHashKey}:items:{cart.Id.ToString()}:sortedset";
            var cartItemHashKey = $"{cartHashKey}:items:{cart.Id.ToString()}";
            var cartItemIds = await _redisService.GetSortedSetAsync(cartItemSortedSetKey);
            foreach (var itemId in cartItemIds)
            {
                var itemFromRedis = await _redisService.GetHashAsync(cartItemHashKey, itemId);
                if (itemFromRedis != null)
                {
                    var cartItem = JsonSerializer.Deserialize<CartItem>(itemFromRedis);
                    if (cartItem != null)
                    {
                        response.CartItems.Add(new CartItemResponse()
                        {
                            Id = cartItem.Id.ToString(),
                            ProductVariantId = cartItem.ProductVariantId.ToString(),
                            ProductNameSnapshot = cartItem.ProductNameSnapshot,
                            ProductVariantNameSnapshot = cartItem.ProductVariantNameSnapshot,
                            Quantity = cartItem.Quantity,
                            UnitPriceAtAdditionSnapshot = (float)cartItem.UnitPriceAtAdditionSnapshot,
                            ItemSubtotalAmount = (float)cartItem.ItemSubtotalAmount
                        });
                        // response.CartItems.Add(cartItemResponse);
                    }
                }
            }
            var promotionSortedSetKey = $"{cartHashKey}:promotion:{cart.Id.ToString()}:sortedset";
            var promotionHashKey = $"{cartHashKey}:promotion:{cart.Id.ToString()}";
            
            var promotionIds = await _redisService.GetSortedSetAsync(promotionSortedSetKey);
            foreach (var promotionId in promotionIds)
            {
                var promotionFromRedis = await _redisService.GetHashAsync(promotionHashKey, promotionId);
                if (promotionFromRedis != null)
                {
                    var promotion = JsonSerializer.Deserialize<CartAppliedPromotionDetail>(promotionFromRedis);
                    if (promotion != null)
                    {
                        response.AppliedPromotions.Add(new CartAppliedPromotionDetailResponse()
                        {
                            Id = promotion.Id.ToString(),
                            PromotionNameSnapshot = promotion.PromotionNameSnapshot,
                            DiscountValueCalculated = (float)promotion.DiscountValueCalculated,
                            PromotionRuleId = promotion.PromotionRuleId.ToString(),
                        });
                    }
                }
            }
        }
        return response;
    }
}