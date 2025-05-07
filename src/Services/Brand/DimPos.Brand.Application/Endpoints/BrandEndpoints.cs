using Carter;
using DimPos.Brand.Application.Features.Brands.Command;
using DimPos.Brand.Domain.Constants;
using DimPos.Brand.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Brand.Application.Endpoints;

public class BrandEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Brands.BrandsEndpoint).WithTags("Brands");
        group.MapPost("", CreateBrand)
            .WithName(nameof(CreateBrand))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateBrand(IMediator mediator, [FromForm] CreateBrandCommand command)
    {
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
}