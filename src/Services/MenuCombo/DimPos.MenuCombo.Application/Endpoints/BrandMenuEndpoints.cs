using Carter;
using DimPos.MenuCombo.Application.Common.Utils;
using DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;
using DimPos.MenuCombo.Application.Features.BrandMenu.Command.UpdateBrandMenu;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuByBrand;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetBrandMenuById;
using DimPos.MenuCombo.Application.Features.BrandMenu.Query.GetStoresByMenu;
using DimPos.MenuCombo.Application.Features.BrandMenuItems.Command.UpdateBrandMenuItems;
using DimPos.MenuCombo.Application.Features.BrandMenuItems.Query.GetProductVariantsByMenu;
using DimPos.MenuCombo.Application.Features.StoreMenuAssignments.Command.AssignStoreMenu;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.MenuCombo.Application.Endpoints;

public class BrandMenuEndpoints : ICarterModule
{
    public BrandMenuEndpoints()
    {
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.BrandMenus.BrandMenusEndpoint).WithTags("BrandMenus");
        group.MapPost("", CreateBrandMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateBrandMenu))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            // .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetBrandMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetBrandMenu))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            // .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdateBrandMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateBrandMenu))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{brandMenuId}", GetBrandMenuById).RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetBrandMenuById))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            // .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{brandMenuId}/product-variants", GetProductVariantsByMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetProductVariantsByMenu))
            .Produces<ApiResponse>(StatusCodes.Status200OK);
        group.MapPatch("/{brandMenuId}/product-variants", UpdateProductVariantsInMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateProductVariantsInMenu))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/{brandMenuId}/stores", GetStoresByMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetStoresByMenu))
            .Produces<ApiResponse>(StatusCodes.Status200OK);
        group.MapPatch("/{brandMenuId}/stores", AssignMenuToStore).RequireAuthorization("BrandPolicy")
            .WithName(nameof(AssignMenuToStore))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateBrandMenu(IMediator mediator, [FromBody] CreateBrandMenuCommand command, 
        ValidationUtil<CreateBrandMenuCommand> validationUtil)
    {
        // Validate the command using the ValidationUtil
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.BrandMenus.BrandMenusEndpoint}", apiResponse);
    }

    public async Task<IResult> GetBrandMenu(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var command = new GetBrandMenuByBrandQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetProductVariantsByMenu(IMediator mediator, [FromRoute] Guid brandMenuId, [FromQuery] int page = 1,
        [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var command = new GetProductVariantsByMenuQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            BrandMenuId = brandMenuId
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> UpdateProductVariantsInMenu(IMediator mediator, [FromRoute] Guid brandMenuId, [FromBody] UpdateBrandMenuItemsRequest request)
    {
        var command = new UpdateBrandMenuItemsCommand()
        {
            BrandMenuId = brandMenuId,
            UpdateBrandMenuItemsRequest = request
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> GetStoresByMenu(IMediator mediator, [FromRoute] Guid brandMenuId, [FromQuery] int page = 1,
        [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,  [FromQuery] string? name = null, [FromQuery] string? code = null)
    {
        var command = new GetStoresByMenuQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            BrandMenuId = brandMenuId,
            Name = name,
            Code = code
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> AssignMenuToStore(IMediator mediator, [FromRoute] Guid brandMenuId, [FromBody] List<AssignStoreMenuRequest> request)
    {
        var command = new AssignStoreMenuCommand()
        {
            BrandMenuId = brandMenuId,
            AssignStoreMenuRequests= request
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }
    public async Task<IResult> GetBrandMenuById(IMediator mediator, [FromRoute] Guid brandMenuId)
    {
        var command = new GetBrandMenuByIdQuery()
        {
            BrandMenuId = brandMenuId
        };
        var apiResponse = await mediator.Send(command);
        return Results.Json(apiResponse);
    }

    public async Task<IResult> UpdateBrandMenu(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateBrandMenuRequest request,
        ValidationUtil<UpdateBrandMenuCommand> validationUtil)
    {
        var command = new UpdateBrandMenuCommand()
        {
            BrandMenuId = id,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type
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