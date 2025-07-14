using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.RemoveRuleCondition;

public class RemoveRuleConditionCommandHandler : IRequestHandler<RemoveRuleConditionCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public RemoveRuleConditionCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(RemoveRuleConditionCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");
        }

        var ruleConditions = await _unitOfWork.GetRepository<RuleConditions>().GetListAsync(
            predicate: x => x.PromotionRuleId == request.PromotionRuleId 
            && x.PromotionRule.BrandId == brandId
        );
        var ruleCondition = ruleConditions.FirstOrDefault(x => x.Id == request.RuleConditionId);
        if (ruleCondition == null)
        {
            throw new BadHttpRequestException("Không tìm thấy điều kiện quy tắc khuyến mãi với ID đã cung cấp.");
        }

        if (ruleConditions.Count <= 1)
        {
            throw new BadHttpRequestException("Không thể xoá điều kiện quy tắc khuyến mãi. Quy tắc khuyến mãi phải có ít nhất một điều kiện.");
        }
        _unitOfWork.GetRepository<RuleConditions>().DeleteAsync(ruleCondition);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Xoá điều kiện quy tắc khuyến mãi không thành công.");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xoá điều kiện quy tắc khuyến mãi thành công",
            Data = null
        };

    }
}