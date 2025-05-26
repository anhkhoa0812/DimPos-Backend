using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Application.Features.Products.Commands.UpdateProducts;
using DimPos.Catalog.Application.Features.Products.Query.GetAllProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
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
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<List<ProductResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);
        group.MapPatch("/{id}", UpdateProductsById)
            .WithName(nameof(UpdateProductsById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateProduct(IMediator mediator,  [FromForm] CreateProductsCommand command, ValidationUtil<CreateProductsCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetProducts(IMediator mediator, [FromQuery] int page = 1, 
        [FromQuery] int size = 10, [FromQuery] string? sortBy = nameof(Products.DisplayOrder), [FromQuery] bool isAsc = true, 
        [FromQuery] EProductStatus? status = null, 
        [FromQuery] string? name = null, [FromQuery] bool? isHasVariants = null)
    {
        var query = new GetAllProductsQueries()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Status = status,
            Name = name,
            IsHasVariants = isHasVariants
        };
        var result = await mediator.Send(query);
        return Results.Json(result);
    }
    public async Task<IResult> UpdateProductsById(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateProductsRequest request, ValidationUtil<UpdateProductsCommand> validationUtil)
    {
        var command = new UpdateProductsCommand()
        {
            ProductId = id,
            UpdateProducts = request
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
}