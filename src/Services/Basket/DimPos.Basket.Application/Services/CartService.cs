using System.Text.Json;
using DimPos.Basket.Application.Enums;
using DimPos.Basket.Application.Models;
using DimPos.Basket.Application.Models.Base;
using DimPos.Basket.Application.Models.Request;
using DimPos.Basket.Application.Models.Response;
using DimPos.Basket.Application.Services.Interface;

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
                ItemSpecificDiscountAmount = 0,
                ItemFinalPrice = request.UnitPriceAtAdditionSnapshot * request.Quantity,
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
        try
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
            };
            var cartJson = JsonSerializer.Serialize(cart);
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
        // var staffAccountId = _claimService.GetCurrentUserId;
        var staffAccountId = Guid.Parse("0197543b-b4c5-7068-934d-e9bc0acefb05");
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
        // var staffAccountId = _claimService.GetCurrentUserId;
        // if (staffAccountId == Guid.Empty)
        // {
        //     throw new BadHttpRequestException("Không tìm thấy tài khoản nhân viên");
        // }
        //
        // var cartHashKey = $"cart:{staffAccountId}";
        // var cartJson = await _redisService.GetHashAsync(cartHashKey, cartId.ToString());
        // if(string.IsNullOrEmpty(cartJson))
        //     throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        //
        // switch (request.PromotionTypeSnapshot)
        // {
        //     case EPromotionType.CartWideDiscount:
        //         var cart = JsonSerializer.Deserialize<Cart>(cartJson);
        //         if (cart == null)
        //         {
        //             throw new BadHttpRequestException("Không tìm thấy giỏ hàng");
        //         }
        //         
        //         break;
        // }
        //
        throw new BadHttpRequestException("");
    }
}