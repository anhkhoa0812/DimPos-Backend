using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.Stores.Command.CreateStaff;
using DimPos.Store.Application.Features.Stores.Command.UpdateStaff;
using DimPos.Store.Application.Features.Stores.Query.GetStaffById;
using DimPos.Store.Application.Features.Stores.Query.GetStaffs;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using DimPos.Store.Domain.Models.Response;
using Microsoft.AspNetCore.Mvc;
using IMediator = Mediator.IMediator;

namespace DimPos.Store.Application.Endpoints;

public class StaffEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group  = app.MapGroup(ApiEndpointConstant.Staff.StaffEndpoint).WithTags("Staff");
        
        group.MapPost("", CreateStaff).RequireAuthorization("StorePolicy").WithName(nameof(CreateStaff))
            .DisableAntiforgery()
            .WithName(nameof(CreateStaff))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetStaffs)
            .WithName(nameof(GetStaffs))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<List<GetStaffsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetStaffById)
            .WithName(nameof(GetStaffById))
            .RequireAuthorization("StorePolicy")
            .Produces<ApiResponse<GetStaffByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdateStaff)
            .DisableAntiforgery()
            .RequireAuthorization("StorePolicy")
            .WithName(nameof(UpdateStaff))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
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
        return Results.Created($"{ApiEndpointConstant.Staff.StaffEndpoint}", result);
    }
    public async Task<IResult> GetStaffs(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetStaffsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> GetStaffById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetStaffByIdQuery() { StaffId = id };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    } 
    
    public async Task<IResult> UpdateStaff(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateStaffRequest request, ValidationUtil<UpdateStaffCommand> validationUtil)
    {
        var command = new UpdateStaffCommand()
        {
            StaffId = id,
            Code = request.Code,
            Username = request.Username,
            Email = request.Email,
            Status = request.Status
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