using Carter;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;
using DimPos.Promotion.Application.Features.PromotionRule.Command.UpdatePromotionRule;
using DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleByCart;
using DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleById;
using DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRules;
using DimPos.Promotion.Application.Features.RuleAction.Command.UpdateRuleAction;
using DimPos.Promotion.Application.Features.RuleCondition.Command.AddRuleCondition;
using DimPos.Promotion.Application.Features.RuleCondition.Command.RemoveRuleCondition;
using DimPos.Promotion.Application.Features.RuleCondition.Command.UpdateRuleCondition;
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
        group.MapGet("{id:guid}", GetPromotionRuleById)
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(GetPromotionRuleById))
            .Produces<GetPromotionRuleByIdResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPatch("{id:guid}", UpdatePromotionRule)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdatePromotionRule))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPost("{promotionRuleId:guid}/rule-conditions", AddRuleCondition)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(AddRuleCondition))
            .Produces<ApiResponse>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{promotionRuleId:guid}/rule-conditions/{ruleConditionId:guid}", UpdateRuleCondition)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateRuleCondition))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapDelete("{promotionRuleId:guid}/rule-conditions/{ruleConditionId:guid}", RemoveRuleCondition)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(RemoveRuleCondition))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        group.MapPut("{promotionRuleId:guid}/rule-actions/{ruleActionId:guid}", UpdateRuleAction)
            .DisableAntiforgery()
            .RequireAuthorization("BrandPolicy")
            .WithName(nameof(UpdateRuleAction))
            .Produces<ApiResponse>(StatusCodes.Status200OK)
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
    public async Task<IResult> GetPromotionRules(IMediator mediator, [FromQuery] int size = 30,
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
    public async Task<IResult> GetPromotionRuleById(IMediator mediator, [FromRoute] Guid id)
    {
        var query = new GetPromotionRuleByIdQuery()
        {
            PromotionRuleId = id
        };
        var apiResponse = await mediator.Send(query);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdatePromotionRule(IMediator mediator, [FromRoute] Guid id, [FromBody] UpdatePromotionRuleRequest request,
        ValidationUtil<UpdatePromotionRuleCommand> validationUtil)
    {
        var command = new UpdatePromotionRuleCommand()
        { 
            PromotionRuleId= id,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            Priority = request.Priority,
            ShortDescription = request.ShortDescription
        };
        
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> UpdateRuleCondition(IMediator mediator, [FromRoute] Guid promotionRuleId,
        [FromRoute] Guid ruleConditionId,
        [FromBody] UpdateRuleConditionRequest request, ValidationUtil<UpdateRuleConditionCommand> validationUtil)
    {
        var command = new UpdateRuleConditionCommand()
        {
            PromotionRuleId = promotionRuleId,
            RuleConditionId = ruleConditionId,
            Operator = request.Operator,
            Value = request.Value,
        };
        
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> RemoveRuleCondition(IMediator mediator, [FromRoute] Guid promotionRuleId, 
        [FromRoute] Guid ruleConditionId)
    {
        var command = new RemoveRuleConditionCommand()
        {
            PromotionRuleId = promotionRuleId,
            RuleConditionId = ruleConditionId
        };
        
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }
    public async Task<IResult> UpdateRuleAction(IMediator mediator, [FromRoute] Guid promotionRuleId,
        [FromRoute] Guid ruleActionId, [FromBody] UpdateRuleActionRequest request,
        ValidationUtil<UpdateRuleActionCommand> validationUtil)
    {
        var command = new UpdateRuleActionCommand()
        {
            PromotionRuleId = promotionRuleId,
            RuleActionId = ruleActionId,
            Value = request.Value,
            MaxDiscountAmountForPercentage = request.MaxDiscountAmountForPercentage,
            TargetCriteriaForItemAction = request.TargetCriteriaForItemAction,
            ActionType = request.ActionType
        };
        
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        
        var apiResponse = await mediator.Send(command);
        return Results.Ok(apiResponse);
    }

    public async Task<IResult> AddRuleCondition(IMediator mediator, [FromRoute] Guid promotionRuleId,
        [FromBody] AddRuleConditionRequest request,
        ValidationUtil<AddRuleConditionCommand> validationUtil)
    {
        var command = new AddRuleConditionCommand()
        {
            PromotionRuleId = promotionRuleId,
            ConditionType = request.ConditionType,
            Operator = request.Operator,
            Value = request.Value
        };
        var (isValid, response) = await validationUtil.ValidateAsync(command);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var apiResponse = await mediator.Send(command);
        return Results.Created(
                $"{ApiEndpointConstants.PromotionRules.PromotionRulesEndpoint}/{promotionRuleId}/rule-conditions"
                ,apiResponse);
    }
}