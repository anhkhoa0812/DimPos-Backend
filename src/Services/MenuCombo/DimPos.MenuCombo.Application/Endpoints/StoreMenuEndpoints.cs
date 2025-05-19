using Carter;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoreMenu;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;

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
    }
    public async Task<IResult> GetStoreMenuByStore(IMediator mediator)
    {
        var command = new GetStoreMenuQuery();
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
}