using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroups;
using DimPos.Catalog.Application.Features.ModifierGroups.Query.GetModifierGroupsById;
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
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetModifierGroups(IMediator mediator,
        [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetModifierGroupsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
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
}