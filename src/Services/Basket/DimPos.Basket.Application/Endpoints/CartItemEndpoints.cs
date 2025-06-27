using Carter;
using DimPos.Basket.Application.Models.Base;
using DimPos.Basket.Application.Models.Request;
using DimPos.Basket.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Basket.Application.Endpoints;

public class CartItemEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart-items").WithTags("CartItems");
        group.MapPatch("{id:guid}", UpdateCartItem)
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> UpdateCartItem([FromRoute] Guid id, [FromBody] UpdateCartItemRequest request, [FromServices] ICartService cartService)
    {
        var apiResponse = await cartService.UpdateCartItemAsync(id, request);
        return Results.Ok(apiResponse);
    }
}