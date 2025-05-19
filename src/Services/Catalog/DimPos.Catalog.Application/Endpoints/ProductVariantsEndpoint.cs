using Carter;
using DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductProductVariantsById;
using DimPos.Catalog.Application.Features.ProductVariants.Query.GetProductVariants;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
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
    }

    public async Task<IResult> GetProductVariants(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, [FromQuery] string? name = null)
    {
        var query = new GetProductVariantsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name
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
}