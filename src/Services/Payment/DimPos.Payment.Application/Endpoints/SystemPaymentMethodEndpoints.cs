using Carter;
using DimPos.Payment.Application.Common.Utils;
using DimPos.Payment.Application.Features.SystemPaymentMethod.Command.UpdateSystemPaymentMethod;
using DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethodById;
using DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethods;
using DimPos.Payment.Domain.Constants;
using DimPos.Payment.Domain.Models.Common;
using DimPos.Payment.Domain.Models.SystemPaymentMethod;
using DimPos.Payment.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Payment.Application.Endpoints;

public class SystemPaymentMethodEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndPointConstants.SystemPaymentMethods.SystemPaymentMethodsEndpoint)
            .WithTags("System Payment Methods");
        group.MapGet("", GetSystemPaymentMethods)
            .WithName(nameof(GetSystemPaymentMethods))
            .RequireAuthorization("AdminBrandStorePolicy")
            .Produces<ApiResponse<IPaginate<GetSystemPaymentMethodsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("{id:guid}", GetSystemPaymentMethodById)
            .WithName(nameof(GetSystemPaymentMethodById))
            .RequireAuthorization("AdminBrandStorePolicy")
            .Produces<ApiResponse<GetSystemPaymentMethodByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdateSystemPaymentMethod)
            .DisableAntiforgery()
            .WithName(nameof(UpdateSystemPaymentMethod))
            .RequireAuthorization("SystemAdminPolicy")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> GetSystemPaymentMethods(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true,
        [FromQuery] string? name = null)
    {
        var query = new GetSystemPaymentMethodsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc,
            Name = name
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetSystemPaymentMethodById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetSystemPaymentMethodByIdQuery()
        {
            SystemPaymentMethodId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateSystemPaymentMethod(IMediator mediator, [FromRoute] Guid id,
        [FromBody] UpdateSystemPaymentMethodRequest request,
        ValidationUtil<UpdateSystemPaymentMethodCommand> validationUtil)
    {
        var command = new UpdateSystemPaymentMethodCommand()
        {
            SystemPaymentMethodId = id,
            Name = request.Name,
            Description = request.Description,
            IsGloballyActive = request.IsGloballyActive
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