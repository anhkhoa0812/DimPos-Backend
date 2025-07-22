using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ComboProducts.Command.CreateComboProduct;
using DimPos.Catalog.Application.Features.ComboProducts.Query.GetAllComboProducts;
using DimPos.Catalog.Application.Features.ComboProducts.Query.GetComboProductById;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.ComboProducts;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ComboProductsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ComboProducts.ComboProductsEndpoint).WithTags("Combo Products");
        group.MapPost("", CreateComboProduct)
            .DisableAntiforgery()
            .WithName(nameof(CreateComboProduct))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetAllComboProducts)
            .WithName(nameof(GetAllComboProducts))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<IPaginate<GetAllComboProductsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetComboProductById)
            .WithName(nameof(GetComboProductById))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<GetComboProductByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
            
    }

    public async Task<IResult> CreateComboProduct(IMediator mediator, [FromForm] CreateComboProductCommand command,
        ValidationUtil<CreateComboProductCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created( $"{ApiEndPointConstants.ComboProducts.ComboProductsEndpoint}" ,apiResponse);
    }
    public async Task<IResult> GetAllComboProducts(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetAllComboProductsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetComboProductById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetComboProductByIdQuery()
        {
            ProductVariantId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}