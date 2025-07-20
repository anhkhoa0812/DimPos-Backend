using Carter;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoreMenu;
using DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuById;
using DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Query.GetStoreMenuByStoreId;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using DimPos.MenuCombo.Domain.Models.StoreMenu;
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
        group.MapGet("stores/{id:guid}", GetStoreMenuByStoreId)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetStoreMenuByStoreId))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetStoreMenuById)
            .WithName(nameof(GetStoreMenuById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<GetStoreMenuByIdResponse>>(StatusCodes.Status200OK)
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

    public async Task<IResult> GetStoreMenuByStoreId(IMediator mediator, [FromRoute] Guid id,
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
    public async Task<IResult> GetStoreMenuById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetStoreMenuByIdQuery()
        {
            StoreMenuId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}