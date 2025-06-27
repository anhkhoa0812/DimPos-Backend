using DimPos.Basket.Application.Models.Base;
using DimPos.Basket.Application.Models.Request;

namespace DimPos.Basket.Application.Services.Interface;

public interface ICartService
{
    public Task<ApiResponse> AddToCartAsync(Guid cartId, AddToCartRequest request);
    public Task<ApiResponse> CreateNewCartAsync(CreateNewCartRequest request);
    public Task<ApiResponse> DeleteCartAsync(Guid cartId);
    
    public Task<ApiResponse> GetCartAsync();

    public Task<ApiResponse> ApplePromotionAsync(Guid cartId, ApplyPromotionRequest request);
    
    public Task<ApiResponse> UpdateCartAsync(Guid cartId, UpdateCartRequest request);
    Task<ApiResponse> UpdateCartItemAsync(Guid cartId, Guid cartItemId, UpdateCartItemRequest request);
    Task<ApiResponse> RemoveAppliedPromotionAsync(Guid cartId, RemoveCartAppliedPromotionDetailRequest request);
}