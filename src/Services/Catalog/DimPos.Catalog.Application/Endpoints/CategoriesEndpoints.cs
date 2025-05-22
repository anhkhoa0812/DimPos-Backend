using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;
using DimPos.Catalog.Application.Features.Categories.Command.UpdateCategories;
using DimPos.Catalog.Application.Features.Categories.Query.GetCategoriesByBrand;
using DimPos.Catalog.Application.Features.Categories.Query.GetCategoryById;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Filter.FilterModel;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class CategoriesEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.Categories.CategoriesEndpoint).WithTags("Categories");

        group.MapPost("", CreateCategory)
            .RequireAuthorization("BrandPolicy")
            .DisableAntiforgery()
            .WithName(nameof(CreateCategory))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetCategoriesByBrand)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetCategoriesByBrand))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id}", GetCategoryById)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetCategoryById))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id}", UpdateCategory)
            .RequireAuthorization("BrandPolicy")
            .DisableAntiforgery()
            .WithName(nameof(UpdateCategory))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateCategory(IMediator mediator, [FromForm] CreateCategoriesCommand command, ValidationUtil<CreateCategoriesCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetCategoriesByBrand(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, [FromQuery] string? name = null)
    {
        var query = new GetCategoriesByBrandQuery()
        {
            Size = size,
            Page = page,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetCategoryById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetCategoryByIdQuery()
        {
            CategoryId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> UpdateCategory(IMediator mediator, [FromRoute] Guid id, [FromForm] UpdateCategoriesRequest request, ValidationUtil<UpdateCategoriesCommand> validationUtil)
    {
        var command = new UpdateCategoriesCommand()
        {
            CategoryId = id,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            ParentCategoryId = request.ParentCategoryId,
            Image = request.Image
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
    
}