using Carter;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Application.Features.Brands.Command;
using DimPos.Brand.Application.Features.Brands.Query.GetBrandDetail;
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
        group.MapGet("/detail", GetBrandDetail)
            .WithName(nameof(GetBrandDetail))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateBrand(IMediator mediator, [FromForm] CreateBrandCommand command, ValidationUtil<CreateBrandCommand> validationUtil)
    {
        // Validate the command using the ValidationUtil
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.Brands.BrandsEndpoint}" ,apiResponse);
    }

    public async Task<IResult> GetBrandDetail(IMediator mediator)
    {
        var query = new GetBrandDetailQuery();
        var apiResponse = await mediator.Send(query);
        
        return Results.Json(apiResponse);
    }
}