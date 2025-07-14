using System.Text.Json;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.UpdateRuleCondition;

public class UpdateRuleConditionCommandHandler : IRequestHandler<UpdateRuleConditionCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateRuleConditionCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
     
    public async ValueTask<ApiResponse> Handle(UpdateRuleConditionCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;

        var ruleCondition = await _unitOfWork.GetRepository<RuleConditions>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.RuleConditionId 
            && x.PromotionRuleId == request.PromotionRuleId 
            && x.PromotionRule.BrandId == brandId
        );

        if (ruleCondition == null)
        {
            throw new BadHttpRequestException("Không tìm thấy điều kiện quy tắc khuyến mãi với ID đã cung cấp.");
        }

        switch (ruleCondition.ConditionType)
        {
            case EConditionType.MinCartValue:
                if(request.Operator != EOperator.GreaterThan &&
                   request.Operator != EOperator.GreaterThanOrEqual)
                {
                    throw new BadHttpRequestException("Đối với điều kiện MinCartValue, chỉ hỗ trợ GreaterThan hoặc GreaterThanOrEqual.");
                }
                if (!decimal.TryParse(request.Value, out var minCartValue))
                {
                    throw new BadHttpRequestException("Giá trị của MinCartValue không hợp lệ");
                }
                if (minCartValue < 0)
                {
                    throw new BadHttpRequestException("Giá trị của MinCartValue phải lớn hơn 0.");
                }
                break;
            case EConditionType.CartContainsProductVariant:
                if (request.Operator != EOperator.ContainsAllInList &&
                    request.Operator != EOperator.ContainsAnyInList &&
                    request.Operator != EOperator.ContainsExactList)
                {
                    throw new BadHttpRequestException("Đối với điều kiện CartContainsProductVariant, chỉ hỗ trợ ContainsAllInList, ContainsAnyInList hoặc ContainsExactList.");
                }
                List<Guid>? variantIds;
                try
                {
                    variantIds = JsonSerializer.Deserialize<List<Guid>>(request.Value);
                }
                catch (JsonException ex)
                {
                    throw new BadHttpRequestException(
                        "Giá trị value của Rule Condition không đúng định dạng", ex);
                }
                if (variantIds == null || !variantIds.Any())
                {
                    throw new BadHttpRequestException("Giá trị của CartContainsProductVariant không được để trống hoặc không hợp lệ.");
                }
                break;
            case EConditionType.QuantityOfSpecificProductVariant:
                if (request.Operator != EOperator.GreaterThan &&
                    request.Operator != EOperator.Equals)
                {
                    throw new BadHttpRequestException("Đối với điều kiện QuantityOfSpecificProductVariant, chỉ hỗ trợ GreaterThan hoặc Equals.");
                }
                QuantityOfSpecificProductVariantModel? model;
                try
                {
                    model = JsonSerializer.Deserialize<QuantityOfSpecificProductVariantModel>(request.Value);
                }
                catch (JsonException ex)
                {
                    throw new BadHttpRequestException(
                        "Giá trị ruleCondition.Value không đúng định dạng JSON của QuantityOfSpecificProductVariantModel.", ex);
                }
                if (model.ProductVariantId == Guid.Empty)
                    throw new BadHttpRequestException("ProductVariantId phải khác Guid.Empty.");

                if (model.Quantity < 0)
                    throw new BadHttpRequestException("Số lượng phải lớn hơn 0.");
                break;
        }
        ruleCondition.Operator = request.Operator;
        ruleCondition.Value = request.Value;
        
        _unitOfWork.GetRepository<RuleConditions>().UpdateAsync(ruleCondition);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật điều kiện quy tắc khuyến mãi không thành công.");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật điều kiện quy tắc khuyến mãi thành công",
            Data = null
        };
    }
}