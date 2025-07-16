using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.Stores.Command.CreateStaff;
using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using DimPos.Store.Application.Features.Stores.Query.GetStaffs;
using DimPos.Store.Application.Features.Stores.Query.GetStoresByBrand;
using DimPos.Store.Application.Features.TaxRate.Command.CreateTaxRate;
using DimPos.Store.Application.Features.TaxRate.Command.UpdateTaxRate;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using DimPos.Store.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class StoreEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.Store.StoreEndpoint).WithTags("Stores");
        group.MapPost("", CreateStore).RequireAuthorization("BrandPolicy").WithName(nameof(CreateStore))
            .DisableAntiforgery()
            .WithName(nameof(CreateStore))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("/staff", CreateStaff).RequireAuthorization("StorePolicy").WithName(nameof(CreateStaff))
            .DisableAntiforgery()
            .WithName(nameof(CreateStaff))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("/{id:guid}/tax-rates", CreateTaxRateForStore)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateTaxRateForStore))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("/{id:guid}/tax-rates/{taxRateId:guid}", UpdateTaxRateForStore)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateTaxRateForStore))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetStoresByBrand)
            .WithName(nameof(GetStoresByBrand))
            .RequireAuthorization("BrandPolicy")
            .Produces<IPaginate<GetStoresByBrandResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/staffs", GetStaffs)
            .WithName(nameof(GetStaffs))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<List<GetStaffsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> CreateStore(IMediator mediator, [FromBody] CreateStoreCommand command, ValidationUtil<CreateStoreCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.Store.StoreEndpoint}/staff", result);
    }

    public async Task<IResult> CreateStaff(IMediator mediator, [FromBody] CreateStaffCommand command,
        ValidationUtil<CreateStaffCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.Store.StoreEndpoint}", result);
    }

    public async Task<IResult> CreateTaxRateForStore(IMediator mediator, [FromRoute] Guid id,
        [FromBody] CreateTaxRateRequest request, ValidationUtil<CreateTaxRateCommand> validationUtil)
    {
        var command = new CreateTaxRateCommand
        {
            StoreId = id,
            Name = request.Name,
            Rate = request.Rate
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.Store.StoreEndpoint}/{id}/tax-rates", result);
    }
    public async Task<IResult> UpdateTaxRateForStore(IMediator mediator, [FromRoute] Guid id, [FromRoute] Guid taxRateId,
        [FromBody] UpdateTaxRateRequest request, ValidationUtil<UpdateTaxRateCommand> validationUtil)
    {
        var command = new UpdateTaxRateCommand
        {
            StoreId = id,
            TaxRateId = taxRateId,
            Name = request.Name,
            Rate = request.Rate,
            IsActive = request.IsActive
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    public async Task<IResult> GetStoresByBrand(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetStoresByBrandQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetStaffs(IMediator mediator)
    {
        var query = new GetStaffsQuery();
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}