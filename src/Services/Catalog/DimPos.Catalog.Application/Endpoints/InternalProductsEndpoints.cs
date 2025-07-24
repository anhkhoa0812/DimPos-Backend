using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.InternalProducts.Command.CreateInternalProduct;
using DimPos.Catalog.Application.Features.InternalProducts.Command.UpdateInternalProduct;
using DimPos.Catalog.Application.Features.InternalProducts.Query.GetInternalProductById;
using DimPos.Catalog.Application.Features.InternalProducts.Query.GetInternalProducts;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.InternalProducts;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Microsoft.AspNetCore.Mvc;
using IMediator = Mediator.IMediator;

namespace DimPos.Catalog.Application.Endpoints;

public class InternalProductsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.InternalProducts.InternalProductsEndpoint).WithTags("InternalProducts");
        group.MapPost("", CreateInternalProduct)
            .DisableAntiforgery()
            .WithName(nameof(CreateInternalProduct))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetInternalProducts)
            .WithName(nameof(GetInternalProducts))
            .RequireAuthorization("BrandAndStorePolicy")
            .Produces<ApiResponse<IPaginate<GetInternalProductResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{id:guid}", GetInternalProductById)
            .WithName(nameof(GetInternalProductById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<GetInternalProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id:guid}", UpdateInternalProducts)
            .DisableAntiforgery()
            .WithName(nameof(UpdateInternalProducts))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
            

    }
    public async Task<IResult> GetInternalProducts(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, [FromQuery] string? name = null,
        [FromQuery] string? sku = null, [FromQuery] string? code = null)
    {
        var query = new GetInternalProductsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name,
            Sku = sku,
            Code = code
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetInternalProductById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetInternalProductByIdQuery()
        {
            ProductVariantId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateInternalProducts(IMediator mediator, [FromRoute] Guid id,
        [FromForm] UpdateInternalProductRequest request,
        ValidationUtil<UpdateInternalProductCommand> validationUtil)
    {
        var command = new UpdateInternalProductCommand()
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Sku = request.Sku,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            ExistInternalProductImages = request.ExistInternalProductImages,
            NewInternalProductImages = request.NewInternalProductImages
        };

        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }

        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> CreateInternalProduct(IMediator mediator, [FromForm] CreateInternalProductCommand command, ValidationUtil<CreateInternalProductCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndPointConstants.Products.ProductsEndpoint}/internal", apiResponse);
    }
}