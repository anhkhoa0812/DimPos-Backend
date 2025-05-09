using Carter;
using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Features.Authentication.Command.Login;
using DimPos.Identity.Domain.Constants;
using DimPos.Identity.Domain.Models.Authentication;
using DimPos.Identity.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Identity.Application.Endpoints;

public class AuthenticationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Authentication.AuthenticationEndpoint).WithTags("Authentication");
        group.MapPost("/login", Login)
            .WithName(nameof(Login))
            .DisableAntiforgery()
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    public async Task<IResult> Login(IMediator mediator, [FromBody] LoginCommand command, ValidationUtil<LoginCommand> validationUtil)
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