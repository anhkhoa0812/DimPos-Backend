using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRules;

public class GetPromotionRulesQueryHandler : IRequestHandler<GetPromotionRulesQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetPromotionRulesQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetPromotionRulesQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var promotionRules = await _unitOfWork.GetRepository<PromotionRules>().GetPagingListAsync(
            selector: x => new GetPromotionRulesResponse()
            {
                Id = x.Id,
                Name = x.Name,
                ShortDescription = x.ShortDescription,
                Description = x.Description,
                Priority = x.Priority,
                IsActive = x.IsActive,
                RuleConditions = x.RuleConditions.Select(rc => new RuleConditionsResponse()
                {
                    Id = rc.Id,
                    ConditionType = rc.ConditionType,
                    Operator = rc.Operator,
                    Value = rc.Value
                }).ToList(),
                RuleActions = new RuleActionsResponse()
                {
                    Id = x.RuleActions.Id,
                    ActionType = x.RuleActions.ActionType,
                    Value = x.RuleActions.Value,
                    TargetCriteriaForItemAction = x.RuleActions.TargetCriteriaForItemAction,
                    MaxDiscountAmountForPercentage = x.RuleActions.MaxDiscountAmountForPercentage
                }
            },
            predicate: x => x.BrandId == brandId && 
                            (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name)),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        return new ApiResponse
        {
            Status = 200,
            Message = "Lấy danh sách quy tắc khuyến mãi thành công",
            Data = promotionRules
        };

    }
}