using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.FinancialShift.Command.CloseFinancialShift;
using DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;
using DimPos.Store.Application.Features.FinancialShift.Query.GetFinancialShiftById;
using DimPos.Store.Application.Features.FinancialShift.Query.GetFinancialShifts;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class FinancialShiftEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.FinancialShift.FinancialShiftEndpoint).WithTags("FinancialShift");
        group.MapPost("/open", OpenFinancialShift)
            .DisableAntiforgery()
            .WithName(nameof(OpenFinancialShift))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("/close", CloseFinancialShift)
            .DisableAntiforgery()
            .WithName(nameof(CloseFinancialShift))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetFinancialShifts)
            .WithName(nameof(GetFinancialShifts))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<IPaginate<GetFinancialShiftsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id:guid}", GetFinancialShiftById)
            .WithName(nameof(GetFinancialShiftById))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<GetFinancialShiftByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> OpenFinancialShift(IMediator mediator, [FromBody] OpenFinancialShiftCommand command,
        ValidationUtil<OpenFinancialShiftCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.FinancialShift.FinancialShiftEndpoint}/open", result);
    }

    public async Task<IResult> GetFinancialShifts(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetFinancialShiftsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    
    public async Task<IResult> GetFinancialShiftById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetFinancialShiftByIdQuery
        {

            FinancialShiftId = id
        };
        var apiResponse = await mediator.Send(query);
        
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> CloseFinancialShift(IMediator mediator)
    {
        var command = new CloseFinancialShiftCommand();
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}