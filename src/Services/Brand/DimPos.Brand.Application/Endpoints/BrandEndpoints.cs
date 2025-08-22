using Carter;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Application.Features.Brands.Command.CreateBrand;
using DimPos.Brand.Application.Features.Brands.Command.UpdateBrands;
using DimPos.Brand.Application.Features.Brands.Command.UpdateBrandsById;
using DimPos.Brand.Application.Features.Brands.Command.UpdatePassword;
using DimPos.Brand.Application.Features.Brands.Query.GetBrandById;
using DimPos.Brand.Application.Features.Brands.Query.GetBrandDetail;
using DimPos.Brand.Application.Features.Brands.Query.GetBrands;
using DimPos.Brand.Domain.Constants;
using DimPos.Brand.Domain.Models.Brand;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Brand.Application.Endpoints;

public class BrandEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Brands.BrandsEndpoint).WithTags("Brands");
        group.MapPost("", CreateBrand)
            .WithName(nameof(CreateBrand))
            .RequireAuthorization("SystemAdminPolicy")
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("/detail", GetBrandDetail)
            .WithName(nameof(GetBrandDetail))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetBrands)
            .WithName(nameof(GetBrands))
            .RequireAuthorization("SystemAdminPolicy")
            .Produces<ApiResponse<IPaginate<GetBrandsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetBrandById)
            .WithName(nameof(GetBrandById))
            .RequireAuthorization("SystemAdminPolicy")
            .Produces<GetBrandByIdResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{id:guid}/passwords", UpdateBrandPassword)
            .DisableAntiforgery()
            .WithName(nameof(UpdateBrandPassword))
            .RequireAuthorization("SystemAdminPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("", UpdateBrands)
            .DisableAntiforgery()
            .WithName(nameof(UpdateBrands))
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdateBrandsById)
            .DisableAntiforgery()
            .WithName(nameof(UpdateBrandsById))
            .RequireAuthorization("SystemAdminPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> CreateBrand(IMediator mediator, [FromForm] CreateBrandCommand command, ValidationUtil<CreateBrandCommand> validationUtil)
    {
        // Validate the command using the ValidationUtil
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.Brands.BrandsEndpoint}" ,apiResponse);
    }

    public async Task<IResult> GetBrandDetail(IMediator mediator)
    {
        var query = new GetBrandDetailQuery();
        var apiResponse = await mediator.Send(query);
        
        return Results.Json(apiResponse);
    }

    public async Task<IResult> GetBrands(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] string? name = null, [FromQuery] string? code = null)
    {
        var query = new GetBrandsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name,
            Code = code
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateBrandPassword(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdatePasswordRequest request,
        ValidationUtil<UpdatePasswordCommand> validationUtil)
    {
        var command = new UpdatePasswordCommand()
        {
            BrandId = id,
            Password = request.Password
        };
        
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetBrandById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetBrandByIdQuery()
        {
            BrandId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdateBrands(IMediator mediator, [FromForm] UpdateBrandsCommand command,
        ValidationUtil<UpdateBrandsCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateBrandsById(IMediator mediator, [FromRoute] Guid id,
        [FromForm] UpdateBrandsByIdRequest request, ValidationUtil<UpdateBrandsByIdCommand> validationUtil)
    {
        var command = new UpdateBrandsByIdCommand()
        {
            BrandId = id,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            Picture = request.Picture
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