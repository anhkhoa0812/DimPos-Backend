using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.CreateFinancialShiftConfig;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;
using DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigById;
using DimPos.Store.Application.Features.FinancialShiftConfig.Query.GetFinancialShiftConfigs;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Paginate.Interface;
using Google.Protobuf.WellKnownTypes;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class FinancialShiftConfigEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.FinancialShiftConfig.FinancialShiftConfigEndpoint).WithTags("FinancialShiftConfig");
        group.MapPost("", CreateFinancialShiftConfig)
            .DisableAntiforgery()
            .WithName(nameof(CreateFinancialShiftConfig))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id}:guid", UpdateFinancialShiftConfig)
            .WithName(nameof(UpdateFinancialShiftConfig))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetFinancialShiftConfig)
            .WithName(nameof(GetFinancialShiftConfig))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<IPaginate<GetFinancialShiftConfigsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetFinancialShiftConfigById)
            .WithName(nameof(GetFinancialShiftConfigById))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<GetFinancialShiftConfigsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateFinancialShiftConfig(IMediator mediator,
        [FromBody] CreateFinancialShiftConfigCommand command,
        ValidationUtil<CreateFinancialShiftConfigCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.FinancialShiftConfig.FinancialShiftConfigEndpoint}", result);
    }
    public async Task<IResult> UpdateFinancialShiftConfig(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateFinancialShiftConfigRequest request,
        ValidationUtil<UpdateFinancialShiftConfigCommand> validationUtil)
    {
        var command = new UpdateFinancialShiftConfigCommand
        {
            FinancialShiftConfigId = id,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            IsActive = request.IsActive
        };

        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    public async Task<IResult> GetFinancialShiftConfig(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetFinancialShiftConfigsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };

        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    public async Task<IResult> GetFinancialShiftConfigById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetFinancialShiftConfigByIdQuery()
        {
            FinancialShiftConfigId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}