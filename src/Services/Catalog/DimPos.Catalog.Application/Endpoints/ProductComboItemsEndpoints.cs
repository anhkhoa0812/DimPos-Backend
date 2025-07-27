using Carter;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.ProductComboItems.Command.UpdateProductComboItem;
using DimPos.Catalog.Domain.Constants;
using DimPos.Catalog.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Catalog.Application.Endpoints;

public class ProductComboItemsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.ProductComboItems.ProductComboItemsEndpoint).WithTags("Product Combo Items");
        
        group.MapPatch("{id:guid}", UpdateProductComboItem)
            .DisableAntiforgery()
            .WithName(nameof(UpdateProductComboItem))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> UpdateProductComboItem(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateProductComboItemRequest request,
        ValidationUtil<UpdateProductComboItemCommand> validationUtil)
    {
        var command = new UpdateProductComboItemCommand()
        {
            ProductComboItemId = id,
            Quantity = request.Quantity,
            DisplayOrder = request.DisplayOrder
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