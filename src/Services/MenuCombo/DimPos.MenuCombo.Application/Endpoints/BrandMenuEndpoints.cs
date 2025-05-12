using Carter;
using DimPos.MenuCombo.Application.Common.Utils;
using DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;
using DimPos.MenuCombo.Domain.Constants;
using DimPos.MenuCombo.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.MenuCombo.Application.Endpoints;

public class BrandMenuEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.BrandMenus.BrandMenusEndpoint).WithTags("BrandMenus");
        group.MapPost("", CreateBrandMenu).RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateBrandMenu))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
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
        return Results.Json(apiResponse);
    }
}