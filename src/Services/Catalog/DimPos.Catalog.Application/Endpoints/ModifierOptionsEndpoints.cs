using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ModifierOptions.Command.UpdateModifierOptions;
using DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsById;
using DimPos.Catalog.Application.Features.ModifierOptions.Query.GetModifierOptionsByModifierGroup;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ModifierOptionsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ModifierOptions.ModifierOptionsEndpoint)
            .WithTags("ModifierOptions");
        group.MapGet("/{id}", GetModifierOptionsById)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetModifierOptionsById))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id}", UpdateModifierOption)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateModifierOption))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> GetModifierOptionsById(
        IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetModifierOptionsByIdQuery()
        {
            ModifierOptionId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> UpdateModifierOption(
        IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateModifierOptionsRequest request, 
        ValidationUtil<UpdateModifierOptionsCommand> validationUtil)
    {
        var command = new UpdateModifierOptionsCommand()
        {
            Id = id,
            UpdateModifierOptions = request
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Json(result);
    }
}