using Carter;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;
using DimPos.Store.Domain.Constants;
using DimPos.Store.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Store.Application.Endpoints;

public class FinancialShiftEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstant.FinancialShift.FinancialShiftEndpoint).WithTags("FinancialShift");
        group.MapPost("/open", OpenFinancialShift)
            .DisableAntiforgery()
            .WithName(nameof(OpenFinancialShift))
            .RequireAuthorization("StaffPolicy")
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> OpenFinancialShift(IMediator mediator, [FromBody] OpenFinancialShiftCommand command,
        ValidationUtil<OpenFinancialShiftCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var result = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstant.FinancialShift.FinancialShiftEndpoint}/open", result);
    }
}