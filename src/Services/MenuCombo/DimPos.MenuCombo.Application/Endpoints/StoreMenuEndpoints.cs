using Carter;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoreMenu;
using DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuByStoreId;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.MenuCombo.Application.Endpoints;

public class StoreMenuEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.StoreMenus.StoreMenusEndpoint).WithTags("StoreMenus");
        group.MapGet("", GetStoreMenuByStore).RequireAuthorization("StorePolicy")
            .WithName(nameof(GetStoreMenuByStore))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("stores/{id:guid}", GetSoreMenuByStoreId)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetSoreMenuByStoreId))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> GetStoreMenuByStore(IMediator mediator)
    {
        var command = new GetStoreMenuQuery();
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetSoreMenuByStoreId(IMediator mediator, [FromRoute] Guid id,
        [FromQuery] int page = 1, [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetStoreMenuByStoreIdQuery()
        {
            StoreId = id,
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}