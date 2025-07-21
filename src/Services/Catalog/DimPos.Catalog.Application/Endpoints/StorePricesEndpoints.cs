using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.StorePrices.Command.UpdateStorePrices;
using DimPos.Catalog.Application.Features.StorePrices.Query.GetAllStorePricesByStoreId;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Domain.Models.StorePrices;
using DimPos.Catalog.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class StorePricesEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.StorePrices.StorePricesEndpoint).WithTags("Store Prices");
        group.MapGet("/stores/{id:guid}", GetStorePricesByStoreId)
            .WithName(nameof(GetStorePricesByStoreId))
            .RequireAuthorization("BrandPolicy")
            .Produces<IPaginate<GetAllStorePricesByStoreIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdateStorePrices)
            .DisableAntiforgery()
            .WithName(nameof(UpdateStorePrices))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    
    public async Task<IResult> GetStorePricesByStoreId(IMediator mediator, [FromRoute] Guid id,
        [FromQuery] int page = 1, [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetAllStorePricesByStoreIdQuery()
        {
            StoreId = id,
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateStorePrices(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateStorePricesRequest request, ValidationUtil<UpdateStorePricesCommand> validationUtil)
    {
        var command = new UpdateStorePricesCommand()
        {
            StorePriceId = id,
            CurrencyCode = request.CurrencyCode,
            OverridePrice = request.OverridePrice
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }
}