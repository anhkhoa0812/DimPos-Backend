using Carter;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;
using DimPos.Promotion.Domain.Constants;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Promotion.Application.Endpoints;

public class CampaignEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.Campaigns.CampaignsEndpoint).WithName("Campaigns");
        group.MapPost("", CreateCampaign)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateCampaign))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    
    public async Task<IResult> CreateCampaign(IMediator mediator, [FromBody] CreateCampaignCommand command, 
        ValidationUtil<CreateCampaignCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.Campaigns.CampaignsEndpoint}", apiResponse);
    }
}