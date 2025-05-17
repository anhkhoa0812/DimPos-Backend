using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;
using DimPos.Catalog.Application.Features.Categories.Query.GetCategoriesByBrand;
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
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetCategoriesByBrand)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetCategoriesByBrand))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
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

    public async Task<IResult> GetCategoriesByBrand(IMediator mediator, [FromQuery] int size, [FromQuery] int page,
        [FromQuery] string? sortBy, [FromQuery] bool isAsc, [FromQuery] string? name)
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

}