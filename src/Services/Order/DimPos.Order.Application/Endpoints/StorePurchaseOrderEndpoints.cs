using Carter;
using DimPos.Order.Application.Common.Utils;
using DimPos.Order.Application.Features.StorePurchaseOrder.Command.CreateStorePurchaseOrder;
using DimPos.Order.Application.Features.StorePurchaseOrder.Command.UpdateStorePurchaseOrder;
using DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrder;
using DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrderById;
using DimPos.Order.Domain.Constants;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Order.Application.Endpoints;

public class StorePurchaseOrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.StorePurchaseOrders.StorePurchaseOrdersEndpoint).WithTags("Store Purchase Orders");

        group.MapPost("", CreateStorePurchaseOrder)
            .DisableAntiforgery()
            .WithName(nameof(CreateStorePurchaseOrder))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetStorePurchaseOrders)
            .WithName(nameof(GetStorePurchaseOrders))
            .RequireAuthorization("BrandAndStorePolicy")
            .Produces<ApiResponse<IPaginate<GetStorePurchaseOrderResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetStorePurchaseOrderById)
            .WithName(nameof(GetStorePurchaseOrderById))
            .RequireAuthorization("BrandAndStorePolicy")
            .Produces<ApiResponse<GetStorePurchaseOrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id:guid}", UpdateStorePurchaseOrder)
            .WithName(nameof(UpdateStorePurchaseOrder))
            .RequireAuthorization("BrandAndStorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateStorePurchaseOrder(IMediator mediator,
        [FromBody] CreateStorePurchaseOrderCommand command,
        ValidationUtil<CreateStorePurchaseOrderCommand> validationUtil)
    {
        var validationResult = await validationUtil.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Response);
        }

        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.StorePurchaseOrders.StorePurchaseOrdersEndpoint}", apiResponse);
    }

    public async Task<IResult> GetStorePurchaseOrders(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var query = new GetStorePurchaseOrderQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            FromDate = fromDate,
            ToDate = toDate
        };
        
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetStorePurchaseOrderById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetStorePurchaseOrderByIdQuery()
        {
            StorePurchaseOrderId = id
        };
        
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdateStorePurchaseOrder(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateStorePurchaseOrderRequest request,
        ValidationUtil<UpdateStorePurchaseOrderCommand> validationUtil)
    {
        var command = new UpdateStorePurchaseOrderCommand()
        {
            StorePurchaseOrderId = id,
            Status = request.Status,
            CancellationReasonByBrand = request.CancellationReasonByBrand,
            CancellationRequestReasonByStore = request.CancellationRequestReasonByStore,
            StorePurchaseOrderItemRequests = request.StorePurchaseOrderItemRequests,
            NoteFromBrand = request.NoteFromBrand
        };
        var validationResult = await validationUtil.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
 }