using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
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
            .Produces<IResult>(StatusCodes.Status201Created)
            .Produces<IResult>(StatusCodes.Status400BadRequest)
            .Produces<IResult>(StatusCodes.Status500InternalServerError);
        
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
}