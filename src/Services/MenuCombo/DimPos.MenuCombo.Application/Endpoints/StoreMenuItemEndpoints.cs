using Carter;
using DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.UpdateStoreMenuItem;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.MenuCombo.Application.Endpoints;

public class StoreMenuItemEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.StoreMenuItems.StoreMenuItemsEndpoint).WithTags("Store Menu Items");
        group.MapPatch("{id:guid}", UpdateStoreMenuItem)
            .DisableAntiforgery()
            .WithName(nameof(UpdateStoreMenuItem))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> UpdateStoreMenuItem(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateStoreMenuItemRequest request)
    {
        var command = new UpdateStoreMenuItemCommand()
        {
            StoreMenuItem = id,
            ProductVariantIds = request.ProductVariantIds
        };
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}