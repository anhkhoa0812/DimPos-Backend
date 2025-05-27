using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class StoreEndpoints  : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.Store.StoreEndpoint).WithTags("Stores");
        group.MapPost("", CreateStore).RequireAuthorization("BrandPolicy").WithName(nameof(CreateStore))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateStore(IMediator mediator, [FromBody] CreateStoreCommand command, ValidationUtil<CreateStoreCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.CreatedAtRoute($"{ApiEndpointConstant.Store.StoreEndpoint}", result);
    }
}