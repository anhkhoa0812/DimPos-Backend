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
}