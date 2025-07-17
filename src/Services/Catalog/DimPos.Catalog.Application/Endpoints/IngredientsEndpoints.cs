using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Ingredients.Command.CreateIngredient;
using DimPos.Catalog.Application.Features.Ingredients.Command.UpdateIngredient;
using DimPos.Catalog.Application.Features.Ingredients.Query.GetIngredientsByBrand;
using DimPos.Catalog.Application.Features.Ingredients.Query.GetIngredientsById;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Ingredients;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class IngredientsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.Ingredients.IngredientsEndpoint).WithTags("Ingredients");
        group.MapPost("", CreateIngredient)
            .DisableAntiforgery()
            .WithName(nameof(CreateIngredient))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetIngredientsByBrand)
            .WithName(nameof(GetIngredientsByBrand))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<IPaginate<GetIngredientsByBrandResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id:guid}", UpdateIngredient)
            .WithName(nameof(UpdateIngredient))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id:guid}", GetIngredientById)
            .WithName(nameof(GetIngredientById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<GetIngredientsByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateIngredient(IMediator mediator, [FromBody] CreateIngredientCommand command,
        ValidationUtil<CreateIngredientCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndPointConstants.Ingredients.IngredientsEndpoint}", apiResponse);
    }

    public async Task<IResult> GetIngredientsByBrand(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetIngredientsByBrandQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateIngredient(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateIngredientRequest request,
        ValidationUtil<UpdateIngredientCommand> validationUtil)
    {
        var command = new UpdateIngredientCommand()
        {
            IngredientId = id,
            Code = request.Code,
            Sku = request.Sku,
            Name = request.Name,
            MeasureUnit = request.MeasureUnit,
            Description = request.Description,
            IsActive = request.IsActive
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetIngredientById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetIngredientsByIdQuery()
        {
            IngredientId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}