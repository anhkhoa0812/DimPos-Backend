using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Domain.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ModifierGroupsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ModifierGroups.ModifierGroupsEndpoint).WithTags("ModifierGroups");

        group.MapPost("", CreateModifierGroup)
            .DisableAntiforgery()
            .WithName(nameof(CreateModifierGroup))
            .Produces<IResult>(StatusCodes.Status201Created)
            .Produces<IResult>(StatusCodes.Status400BadRequest)
            .Produces<IResult>(StatusCodes.Status500InternalServerError);
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
}