using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleById;

public class GetPromotionRuleByIdQueryHandler : IRequestHandler<GetPromotionRuleByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetPromotionRuleByIdQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetPromotionRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");

        var promotionRule = await _unitOfWork.GetRepository<PromotionRules>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.PromotionRuleId && x.BrandId == brandId,
            include: x => x.Include(x => x.RuleActions)
                .Include(x => x.RuleConditions)
                .Include(x => x.CampaignRuleLinks)
                .ThenInclude(x => x.Campaign)
        );
        if (promotionRule == null)
        {
            throw new BadHttpRequestException("Không tìm thấy quy tắc khuyến mãi.");
        }
        
        var response = new GetPromotionRuleByIdResponse()
        {
            Id = promotionRule.Id,
            Name = promotionRule.Name,
            Description = promotionRule.Description,
            ShortDescription = promotionRule.ShortDescription,
            Priority = promotionRule.Priority,
            IsActive = promotionRule.IsActive,
            RuleActions = new RuleActionsResponse()
            {
                Id = promotionRule.RuleActions.Id,
                ActionType = promotionRule.RuleActions.ActionType,
                Value = promotionRule.RuleActions.Value,
                TargetCriteriaForItemAction = promotionRule.RuleActions.TargetCriteriaForItemAction,
                MaxDiscountAmountForPercentage = promotionRule.RuleActions.MaxDiscountAmountForPercentage
            },
            RuleConditions = promotionRule.RuleConditions.Select(rc => new RuleConditionsResponse()
            {
                Id = rc.Id,
                ConditionType = rc.ConditionType,
                Value = rc.Value,
                Operator = rc.Operator
            }).ToList(),
            Campaigns = promotionRule.CampaignRuleLinks?.Select(crl => new CampaignPromotionRuleResponse()
            {
                Id = crl.Campaign.Id,
                Name = crl.Campaign.Name,
                Description = crl.Campaign.Description,
                Priority = crl.Campaign.Priority,
                StartDate = crl.Campaign.StartDate,
                EndDate = crl.Campaign.EndDate,
                IsActive = crl.Campaign.IsActive,
                Channel = crl.Campaign.Channel,
                MaxTotalUsageLimit = crl.Campaign.MaxTotalUsageLimit,
                MaxUsagePerCustomerLimit = crl.Campaign.MaxUsagePerCustomerLimit
            }).ToList()
        };
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy thông tin quy tắc khuyến mãi thành công",
            Data = response
        };
    }
}