using Carter;
using DimPos.Inventory.Application.Features.InventoryStock.Command.RollbackInventoryForOrder;
using DimPos.Inventory.Domain.Constants;
using DimPos.Inventory.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Inventory.Application.Endpoints;

public class InventoryStockEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.InventoryStock.InventoryStockEndpoint).WithTags("Inventory Stock");
        group.MapPatch("orders/{id:guid}/rollback", RollbackInventoryForOrder)
            .DisableAntiforgery()
            .WithName(nameof(RollbackInventoryForOrder))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> RollbackInventoryForOrder(IMediator mediator, [FromRoute] Guid id)
    {
        var command = new RollbackInventoryForOrderCommand() { OrderId = id };
        var response = await mediator.Send(command);
        return Results.Ok(response);
    }
}