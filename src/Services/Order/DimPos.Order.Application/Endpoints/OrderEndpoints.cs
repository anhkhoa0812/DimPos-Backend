using Carter;
using DimPos.Order.Application.Common.Utils;
using DimPos.Order.Application.Features.Order.Command.CreateOrder;
using DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;
using DimPos.Order.Application.Features.Order.Query.GetOrder;
using DimPos.Order.Application.Features.Order.Query.GetOrderWithId;
using DimPos.Order.Domain.Constants;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Order.Application.Endpoints;

public class OrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Orders.OrdersEndpoint).WithTags("Orders");
        group.MapPost("", CreateOrder)
            .DisableAntiforgery()
            .WithName(nameof(CreateOrder))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetOrder)
            .WithName(nameof(GetOrder))
            .RequireAuthorization("BrandAndStoreAndStaffPolicy")
            .Produces<ApiResponse<IPaginate<GetOrderByStoreResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetOrderWithId)
            .WithName(nameof(GetOrderWithId))
            .RequireAuthorization("BrandAndStoreAndStaffPolicy")
            .Produces<ApiResponse<GetOrderWithIdByStoreResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id:guid}/payment-method", UpdatePaymentMethod)
            .WithName(nameof(UpdatePaymentMethod))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateOrder(IMediator mediator, [FromBody] CreateOrderCommand command,
        ValidationUtil<CreateOrderCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.Orders.OrdersEndpoint}", apiResponse);
    }
    public async Task<IResult> GetOrder(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] EOrderStatus? status = null, [FromQuery] EOrderType? type = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var query = new GetOrderQuery()
        {
            Page = page,
            Size = pageSize,
            SortBy = sortBy,
            IsAsc = isAsc,
            Status = status,
            Type = type
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetOrderWithId(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetOrderWithIdQuery()
        {
            OrderId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdatePaymentMethod(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdatePaymentMethodRequest request,
        ValidationUtil<UpdatePaymentMethodCommand> validationUtil)
    {
        var command = new UpdatePaymentMethodCommand()
        {
            OrderId = id,
            StorePaymentMethodConfigId = request.StorePaymentMethodConfigId
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}