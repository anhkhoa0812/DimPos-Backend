using Carter;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;
using DimPos.Promotion.Application.Features.Campaign.Query.GetCampaigns;
using DimPos.Promotion.Application.Features.CampaignStore.Command;
using DimPos.Promotion.Domain.Constants;
using DimPos.Promotion.Domain.Models.Campaigns;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Paginate.Interface;
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
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("{id:guid}/campaign-stores", CreateCampaignStore)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreateCampaignStore))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetCampaigns)
            .RequireAuthorization("BrandPolicy")
            .Produces<ApiResponse<IPaginate<GetCampaignsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
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

    public async Task<IResult> CreateCampaignStore(IMediator mediator, [FromRoute] Guid id,
        [FromBody] CreateCampaignStoreRequest request, ValidationUtil<CreateCampaignStoreCommand> validationUtil)
    {
        var command = new CreateCampaignStoreCommand
        {
            CampaignId = id,
            StoreIds = request.StoreIds
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.Campaigns.CampaignsEndpoint}/{id}/stores", apiResponse);
    }
    public async Task<IResult> GetCampaigns(IMediator mediator, [FromQuery] int page = 1, [FromQuery] int size = 30,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true)
    {
        var query = new GetCampaignsQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            IsAsc = isAsc
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
}