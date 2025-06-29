using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.CreateFinancialShiftConfig;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
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
}