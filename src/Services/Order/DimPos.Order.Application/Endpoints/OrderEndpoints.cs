using Carter;
using DimPos.Order.Application.Common.Utils;
using DimPos.Order.Application.Features.Order.Command.CreateOrder;
using DimPos.Order.Application.Features.Order.Query.GetOrderByStore;
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
        group.MapGet("", GetOrderByStore)
            .WithName(nameof(GetOrderByStore))
            .RequireAuthorization("StoreAndStaffPolicy")
            .Produces<ApiResponse<IPaginate<GetOrderByStoreResponse>>>(StatusCodes.Status200OK)
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
    public async Task<IResult> GetOrderByStore(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] EOrderStatus? status = null, [FromQuery] EOrderType? type = null)
    {
        var query = new GetOrderByStoreQuery()
        {
            Page = page,
            Size = pageSize,
            SortBy = sortBy,
            IsAsc = isAsc,
            Status = status,
            Type = type
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }
}