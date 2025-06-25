using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;

public class CreatePromotionRuleCommandHandler : IRequestHandler<CreatePromotionRuleCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public CreatePromotionRuleCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(CreatePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id thương hiệu");
        }

        var promotionRuleId = Guid.CreateVersion7();
        var promotionRule = new PromotionRules()
        {
            Id = promotionRuleId,
            Name = request.Name,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            BrandId = brandId,
            Priority = request.Priority,
            IsActive = true,
            RuleConditions = request.RuleConditions.Select(x => new RuleConditions()
            {
                Id = Guid.CreateVersion7(),
                ConditionType = x.ConditionType,
                Operator = x.Operator,
                Value = x.Value,
                PromotionRuleId = promotionRuleId
            }).ToList(),
            RuleActions = new RuleActions()
            {
                Id = Guid.CreateVersion7(),
                ActionType = request.RuleActions.ActionType,
                Value = request.RuleActions.Value,
                TargetCriteriaForItemAction = request.RuleActions.TargetCriteriaForItemAction,
                MaxDiscountAmountForPercentage = request.RuleActions.MaxDiscountAmountForPercentage,
                PromotionRuleId = promotionRuleId
            }
        };
        await _unitOfWork.GetRepository<PromotionRules>().InsertAsync(promotionRule);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Tạo luật khuyến mãi không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo luật khuyến mãi thành công",
            Data = null
        };
    }
}