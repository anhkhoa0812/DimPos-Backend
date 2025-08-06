using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ProductVariants.Command.UpdateProductVariants;
using DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductProductVariantsById;
using DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductVariants;
using DimPos.Catalog.Application.Features.RecipeItems.Command.CreateRecipeItem;
using DimPos.Catalog.Application.Features.RecipeItems.Command.RemoveRecipeItem;
using DimPos.Catalog.Application.Features.RecipeItems.Command.UpdateRecipeItem;
using DimPos.Catalog.Application.Features.RecipeItems.Query.GetRecipeItemByProductVariant;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.RecipeItems;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ProductVariantsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ProductVariants.ProductVariantsEndpoint).WithTags("Product Variants");
        group.MapGet("/", GetProductVariants)
            .WithName(nameof(GetProductVariants))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id}", GetProductVariantsById)
            .WithName(nameof(GetProductVariantsById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id}", UpdateProductVariantsById)
            .WithName(nameof(UpdateProductVariantsById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("/{id:guid}/recipe-items", CreateRecipeItem)
            .DisableAntiforgery()
            .WithName(nameof(CreateRecipeItem))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id:guid}/recipe-items", GetRecipeItemsByProductVariant)
            .WithName(nameof(GetRecipeItemsByProductVariant))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<IPaginate<GetRecipeItemByProductVariantResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("/{productVariantId:guid}/recipe-items/{recipeItemId:guid}", UpdateRecipeItem)
            .DisableAntiforgery()
            .WithName(nameof(UpdateRecipeItem))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapDelete("/{productVariantId:guid}/recipe-items/{recipeItemId:guid}", RemoveRecipeItem)
            .DisableAntiforgery()
            .WithName(nameof(RemoveRecipeItem))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> GetProductVariants(IMediator mediator, [FromQuery] int page = 1, 
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, 
        [FromQuery] string? name = null, [FromQuery] string? sku = null, [FromQuery] string? code = null)
    {
        var query = new GetProductVariantsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name,
            Sku = sku,
            Code = code
        };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    public async Task<IResult> GetProductVariantsById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetProductProductVariantsByIdQuery()
        {
            ProductVariantId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdateProductVariantsById(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateProductVariantsRequest request, ValidationUtil<UpdateProductVariantsCommand> validationUtil)
    {
        var command = new UpdateProductVariantsCommand()
        {
            ProductVariantId = id,
            UpdateProductVariants = request
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> CreateRecipeItem(IMediator mediator, [FromRoute] Guid id, [FromBody] CreateRecipeItemRequest request,
        ValidationUtil<CreateRecipeItemCommand> validationUtil)
    {
        var command = new CreateRecipeItemCommand()
        {
            ProductVariantId = id,
            IngredientId = request.IngredientId,
            Quantity = request.Quantity
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndPointConstants.ProductVariants.ProductVariantsEndpoint}/{id}/recipe-items", apiResponse);
    }
    public async Task<IResult> GetRecipeItemsByProductVariant(IMediator mediator, [FromRoute] Guid id,
        [FromQuery] int page = 1, [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetRecipeItemByProductVariantQuery()
        {
            ProductVariantId = id,
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdateRecipeItem(IMediator mediator, [FromRoute] Guid productVariantId, [FromRoute] Guid recipeItemId, [FromBody] UpdateRecipeItemRequest request,
        ValidationUtil<UpdateRecipeItemCommand> validationUtil)
    {
        var command = new UpdateRecipeItemCommand()
        {
            ProductVariantId = productVariantId,
            RecipeItemId = recipeItemId,
            Quantity = request.Quantity,
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> RemoveRecipeItem(IMediator mediator, [FromRoute] Guid productVariantId,
        [FromRoute] Guid recipeItemId)
    {
        var command = new RemoveRecipeItemCommand()
        {
            ProductVariantId = productVariantId,
            RecipeItemId = recipeItemId
        };
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}