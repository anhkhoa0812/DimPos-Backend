using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ExtraProducts.Command.CreateExtraProduct;
using DimPos.Catalog.Application.Features.ExtraProducts.Command.UpdateExtraProduct;
using DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProductById;
using DimPos.Catalog.Application.Features.ExtraProducts.Query.GetExtraProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.ExtraProducts;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ExtraProductsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ExtraProducts.ExtraProductsEndpoint).WithTags("Extra Products");
        
        group.MapPost("", CreateExtraProduct)
            .DisableAntiforgery()
            .WithName(nameof(CreateExtraProduct))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        
        group.MapPatch("{id:guid}", UpdateExtraProduct)
            .DisableAntiforgery()
            .WithName(nameof(UpdateExtraProduct))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetExtraProducts)
            .WithName(nameof(GetExtraProducts))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<IPaginate<GetExtraProductsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetExtraProductsById)
            .WithName(nameof(GetExtraProductsById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<GetExtraProductByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateExtraProduct(IMediator mediator, [FromBody] CreateExtraProductCommand command,
        ValidationUtil<CreateExtraProductCommand> validationUtil)
    {
        var validationResult = await validationUtil.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult);
        }
        
        var apiResponse = await mediator.Send(command);
        
        return Results.Created(ApiEndPointConstants.ExtraProducts.ExtraProductsEndpoint, apiResponse);
    }

    public async Task<IResult> UpdateExtraProduct(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdateExtraProductRequest requset,
        ValidationUtil<UpdateExtraProductCommand> validationUtil)
    {
        var command = new UpdateExtraProductCommand()
        {
            Id = id,
            Name = requset.Name,
            Description = requset.Description,
            DisplayOrder = requset.DisplayOrder,
            IsActive = requset.IsActive,
            Price = requset.Price
        };
        var validationResult = await validationUtil.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult);
        }
        var apiResponse = await mediator.Send(command);
        
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetExtraProducts(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] string? name = null, [FromQuery] string? sku = null)
    {
        var query = new GetExtraProductsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name,
            Sku = sku
        };
        var apiResponse = await mediator.Send(query);
        
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetExtraProductsById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetExtraProductByIdQuery()
        {
            ProductVariantId = id
        };
        var apiResponse = await mediator.Send(query);
        
        return Results.Ok(apiResponse);
    }
}