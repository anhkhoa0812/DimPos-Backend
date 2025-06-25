using Carter;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;
using DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleByCart;
using DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRules;
using DimPos.Promotion.Domain.Constants;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Paginate.Interface;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DimPos.Promotion.Application.Endpoints;

public class PromotionRuleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpointConstants.PromotionRules.PromotionRulesEndpoint).WithName("PromotionRules");
        
        group.MapPost("", CreatePromotionRule)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(CreatePromotionRule))
            .DisableAntiforgery()
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("carts/{cartId:guid}", GetPromotionRuleByCart)
            .RequireAuthorization("StaffPolicy")
            .WithName(nameof(GetPromotionRuleByCart))
            .Produces<ApiResponse<List<PromotionRulesByCartResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapGet("", GetPromotionRules)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetPromotionRules))
            .Produces<ApiResponse<IPaginate<GetPromotionRulesResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
    }
    
    public async Task<IResult> CreatePromotionRule(IMediator mediator, [FromBody] CreatePromotionRuleCommand command, 
        ValidationUtil<CreatePromotionRuleCommand> validationUtil)
    {
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created($"{ApiEndpointConstants.PromotionRules.PromotionRulesEndpoint}", apiResponse);
    }
    public async Task<IResult> GetPromotionRuleByCart(IMediator mediator, [FromRoute] Guid cartId)
    {
        var query = new GetPromotionRuleByCartQuery()
        {
            CartId = cartId
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> GetPromotionRules(IMediator mediator, [FromQuery] int size = 10,
        [FromQuery] int page = 1, [FromQuery] string? sortBy = null, [FromQuery] bool isAsc = true, 
        [FromQuery] string? name = null)
    {
        var query = new GetPromotionRulesQuery()
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
}