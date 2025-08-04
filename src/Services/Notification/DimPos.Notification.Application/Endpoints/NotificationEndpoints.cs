using Carter;
using DimPos.Notification.Application.Features.NotificationRecipient.Command.MakeReadAllNotifications;
using DimPos.Notification.Application.Features.NotificationRecipient.Command.RemoveAllNotifications;
using DimPos.Notification.Application.Features.NotificationRecipient.Query.GetNotifications;
using DimPos.Notification.Domain.Constants;
using DimPos.Notification.Domain.Models.Common;
using DimPos.Notification.Domain.Models.Response;
using DimPos.Notification.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Notification.Application.Endpoints;

public class NotificationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Notification.NotificationEndpoint).WithTags("Notification");
        
        group.MapGet("", GetNotifications)
            .WithName(nameof(GetNotifications))
            .RequireAuthorization()
            .Produces<IPaginate<GetNotificationsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("/make-read", MakeReadAllNotifications)
            .WithName(nameof(MakeReadAllNotifications))
            .RequireAuthorization()
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapDelete("", RemoveAllNotifications)
            .WithName(nameof(RemoveAllNotifications))
            .RequireAuthorization()
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }

    public async Task<IResult> GetNotifications(IMediator mediator, [FromQuery] int page = 1,
        [FromQuery] int size = 30, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetNotificationsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);

        return Results.Ok(apiResponse);
    }
    public async Task<IResult> MakeReadAllNotifications(IMediator mediator)
    {
        var command = new MakeReadAllNotificationsCommand();
        var apiResponse = await mediator.Send(command);

        return Results.Ok(apiResponse);
    }
    public async Task<IResult> RemoveAllNotifications(IMediator mediator)
    {
        var command = new RemoveAllNotificationsCommand();
        var apiResponse = await mediator.Send(command);

        return Results.Ok(apiResponse);
    }
}