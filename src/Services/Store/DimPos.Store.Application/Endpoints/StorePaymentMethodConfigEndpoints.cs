using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.CreateStorePaymentMethodConfig;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.UpdateStorePaymentConfig;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Query.GetStorePaymentMethodConfig;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Query.GetStorePaymentMethodConfigForPos;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using Google.Protobuf.WellKnownTypes;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class StorePaymentMethodConfigEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.StorePaymentMethodConfig.StorePaymentMethodConfigEndpoint)
            .WithTags("StorePaymentMethodConfig");
        group.MapPost("", CreateStorePaymentMethodConfig)
            .DisableAntiforgery()
            .WithName(nameof(CreateStorePaymentMethodConfig))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>( StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/pos", GetStorePaymentMethodConfigForPos)
            .WithName(nameof(GetStorePaymentMethodConfigForPos))
            .RequireAuthorization("StoreAndStaffPolicy")
            .Produces<ApiResponse<List<GetStorePaymentMethodConfigForPosResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetStorePaymentMethodConfig)
            .WithName(nameof(GetStorePaymentMethodConfig))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<List<GetStorePaymentMethodConfigResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id:guid}", UpdateStorePaymentMethodConfig)
            .DisableAntiforgery()
            .RequireAuthorization("StorePolicy")
            .WithName(nameof(UpdateStorePaymentMethodConfig))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateStorePaymentMethodConfig(IMediator mediator,
        [FromBody] CreateStorePaymentMethodConfigCommand command,
        ValidationUtil<CreateStorePaymentMethodConfigCommand> validatorUtil)
    {
        var (isValid, response) = await validatorUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.StorePaymentMethodConfig.StorePaymentMethodConfigEndpoint}", apiResponse);
    }
    public async Task<IResult> GetStorePaymentMethodConfigForPos(IMediator mediator)
    {
        var query = new GetStorePaymentMethodConfigForPosQuery();
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetStorePaymentMethodConfig(IMediator mediator)
    {
        var query = new GetStorePaymentMethodConfigQuery();
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateStorePaymentMethodConfig(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateStorePaymentConfigRequest request, ValidationUtil<UpdateStorePaymentConfigCommand> validatorUtil)
    {
        var command = new UpdateStorePaymentConfigCommand
        {
            StorePaymentMethodConfigId = id,
            IsActiveByStore = request.IsActiveByStore,
        };
        
        var (isValid, response) = await validatorUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}