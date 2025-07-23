using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroups;
using DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroups;
using DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroupsById;
using DimPos.Catalog.Application.Features.ModifierOptions.Command.CreateModifierOption;
using DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsByModifierGroup;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ModifierGroupsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint).WithTags("ModifierGroups");

        group.MapPost("", CreateModifierGroup)
            .RequireAuthorization("BrandPolicy")
            .DisableAntiforgery()
            .WithName(nameof(CreateModifierGroup))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetModifierGroups)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetModifierGroups))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id}", GetModifierGroupById)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetModifierGroupById))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id}/modifier-options", GetModifierOptionsByModifierGroupId)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetModifierOptionsByModifierGroupId))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("/{id}/modifier-options", CreateModifierOption)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateModifierOption))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id}", UpdateModifierGroup)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateModifierGroup))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateModifierGroup (IMediator mediator, 
        [FromBody] CreateModifierGroupsCommand command, ValidationUtil<CreateModifierGroupsCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint}", apiResponse);
    }

    public async Task<IResult> GetModifierGroups(IMediator mediator,
        [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, [FromQuery] string? name = null)
    {
        var query = new GetModifierGroupsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> GetModifierGroupById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetModifierGroupsByIdQuery()
        {
            ModifierGroupId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> GetModifierOptionsByModifierGroupId(
        IMediator mediator, [FromRoute] Guid id,
        [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] string? name = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetModifierOptionsByModifierGroupQuery()
        {
            ModifierGroupsId = id,
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name,
            IsActive = isActive,
        };
        var result = await mediator.Send(query);
        return Results.Json(result);
    }
    public async Task<IResult> UpdateModifierGroup(IMediator mediator, [FromRoute] Guid id, 
        [FromBody] UpdateModifierGroupsRequest request, ValidationUtil<UpdateModifierGroupsCommand> validationUtil)
    {
        var command = new UpdateModifierGroupsCommand()
        {
            Id = id,
            UpdateModifierGroupsRequest = request
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> CreateModifierOption(IMediator mediator, [FromRoute] Guid id,
        [FromBody] CreateModifierOptionRequest request, ValidationUtil<CreateModifierOptionCommand> validationUtil)
    {
        var command = new CreateModifierOptionCommand()
        {
            ModifierGroupId = id,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            PriceDelta = request.PriceDelta
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint}/{id}/modifier-options", apiResponse);
    }
}