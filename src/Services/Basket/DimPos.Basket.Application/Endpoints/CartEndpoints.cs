using Carter;
using DimPos.Basket.Application.Models.Base;
using DimPos.Basket.Application.Models.Request;
using DimPos.Basket.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Basket.Application.Endpoints;

public class CartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/carts").WithTags("Cart");
        group.MapPost("", CreateCart)
            .RequireAuthorization("StaffPolicy")
            .WithName(nameof(CreateCart))
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapPost("{id}/cartItems", AddToCart)
            .RequireAuthorization("StaffPolicy")
            .WithName(nameof(AddToCart))
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        group.MapDelete("{id}", DeleteCart)
            .WithName(nameof(DeleteCart))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetCart)
            .WithName(nameof(GetCart))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK);
        group.MapPost("{id:guid}/promotions", ApplyPromotionToCart)
            .WithName(nameof(ApplyPromotionToCart))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateCart([FromBody] CreateNewCartRequest request, [FromServices] ICartService cartService)
    {
        await cartService.CreateNewCartAsync(request);
        return Results.CreatedAtRoute(nameof(CreateCart));
    }
    public async Task<IResult> AddToCart([FromRoute] Guid id, [FromBody] AddToCartRequest request, [FromServices] ICartService cartService)
    {
        await cartService.AddToCartAsync(id, request);
        return Results.CreatedAtRoute(nameof(AddToCart));
    }

    public async Task<IResult> DeleteCart([FromRoute] Guid id, [FromServices] ICartService cartService)
    {
        var apiResponse = await cartService.DeleteCartAsync(id);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetCart([FromServices] ICartService cartService)
    {
        var apiResponse = await cartService.GetCartAsync();
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> ApplyPromotionToCart([FromRoute] Guid id, [FromBody] ApplyPromotionRequest request, [FromServices] ICartService cartService)
    {
        var apiResponse = await cartService.ApplePromotionAsync(id, request);
        return Results.Created($"/api/carts/{id}/promotions", apiResponse);
    }
}