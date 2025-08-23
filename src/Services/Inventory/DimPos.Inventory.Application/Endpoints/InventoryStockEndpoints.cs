using Carter;
using DimPos.Inventory.Application.Common.Utils;
using DimPos.Inventory.Application.Features.InventoryStock.Command.RollbackInventoryForOrder;
using DimPos.Inventory.Application.Features.InventoryStock.Command.UpdateQuantiyOfInventoryStock;
using DimPos.Inventory.Application.Features.InventoryStock.Query.ExportInventoryStockExcel;
using DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStockById;
using DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStocks;
using DimPos.Inventory.Application.Features.InventoryTransaction.Query.GetInventoryTransactions;
using DimPos.Inventory.Domain.Constants;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Domain.Models.Response;
using DimPos.Inventory.Infrastructure.Paginate.Interface;
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
        group.MapGet("", GetInventoryStocks)
            .WithName(nameof(GetInventoryStocks))
            .RequireAuthorization("StorePolicy")
            .Produces<IPaginate<GetInventoryStocksResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetInventoryStockById)
            .WithName(nameof(GetInventoryStockById))
            .RequireAuthorization("StorePolicy")
            .Produces<GetInventoryStockByIdResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}/inventory-transactions", GetInventoryTransactions)
            .WithName(nameof(GetInventoryTransactions))
            .RequireAuthorization("StorePolicy")
            .Produces<IPaginate<GetInventoryTransactionsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id:guid}/quantity", UpdateQuantityOfInventoryStock)
            .DisableAntiforgery()
            .WithName(nameof(UpdateQuantityOfInventoryStock))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("export/excel", ExportInventoryStockExcel)
            .WithName(nameof(ExportInventoryStockExcel))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
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
    public async Task<IResult> GetInventoryStocks(IMediator mediator, [FromQuery] int page = 1,
    [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetInventoryStocksQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var response = await mediator.Send(query);
        return Results.Ok(response);
    }
    public async Task<IResult> GetInventoryStockById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetInventoryStockByIdQuery()
        {
            InventoryStockId = id
        };
        var response = await mediator.Send(query);
        return Results.Ok(response);
    }

    public async Task<IResult> GetInventoryTransactions(IMediator mediator, [FromRoute] Guid id, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var query = new GetInventoryTransactionsQuery()
        {
            InventoryStockId = id,
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            FromDate = fromDate,
            ToDate = toDate
        };
        var response = await mediator.Send(query);
        return Results.Ok(response);
    }

    public async Task<IResult> UpdateQuantityOfInventoryStock(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateQuantityOfInventoryStockRequest request,
        ValidationUtil<UpdateQuantityOfInventoryStockCommand> validationUtil)
    {
        var command = new UpdateQuantityOfInventoryStockCommand()
        {
            InventoryStockId = id,
            Quantity = request.Quantity,
            Note = request.Note,
            ReasonManualAdjustment = request.ReasonManualAdjustment
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> ExportInventoryStockExcel(IMediator mediator)
    {
        var query = new ExportInventoryStockExcelQuery();
        var response = await mediator.Send(query);
        return Results.Ok(response);
    }
}