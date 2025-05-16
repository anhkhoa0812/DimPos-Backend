using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.Product;
using FluentValidation.Results;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ProductsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.Products.ProductsEndpoint).WithTags("Products");
        group.MapPost("", CreateProduct).WithName(nameof(CreateProduct))
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetProducts).WithName(nameof(GetProducts))
            .Produces<ApiResponse<List<ProductResponse>>>(StatusCodes.Status200OK);
    }
    public async Task<IResult> CreateProduct(IMediator mediator,  [FromBody] CreateProductsCommand command, ValidationUtil<CreateProductsCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetProducts(IMediator mediator)
    {
        var query = new GetAllProductsQueries();
        var result = await mediator.Send(query);
        return Results.Json(result);
    }
}