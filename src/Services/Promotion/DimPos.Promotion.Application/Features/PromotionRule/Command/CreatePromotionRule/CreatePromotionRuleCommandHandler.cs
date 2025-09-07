using System.Text.Json;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
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

        foreach (var ruleCondition in request.RuleConditions)
        {
            switch (ruleCondition.ConditionType)
            {
                case EConditionType.MinCartValue:
                    if(ruleCondition.Operator != EOperator.GreaterThan &&
                       ruleCondition.Operator != EOperator.GreaterThanOrEqual)
                    {
                        throw new BadHttpRequestException("Đối với điều kiện MinCartValue, chỉ hỗ trợ GreaterThan hoặc GreaterThanOrEqual.");
                    }

                    if (!decimal.TryParse(ruleCondition.Value, out var minCartValue))
                    {
                        throw new BadHttpRequestException("Giá trị của MinCartValue không hợp lệ");
                    }
                    if (minCartValue < 0)
                    {
                        throw new BadHttpRequestException("Giá trị của MinCartValue phải lớn hơn 0.");
                    }
                    break;
                case EConditionType.CartContainsProductVariant:
                    if (ruleCondition.Operator != EOperator.ContainsAllInList &&
                        ruleCondition.Operator != EOperator.ContainsAnyInList &&
                        ruleCondition.Operator != EOperator.ContainsExactList)
                    {
                        throw new BadHttpRequestException("Đối với điều kiện CartContainsProductVariant, chỉ hỗ trợ ContainsAllInList, ContainsAnyInList hoặc ContainsExactList.");
                    }
                    List<Guid>? variantIds;
                    try
                    {
                        variantIds = JsonSerializer.Deserialize<List<Guid>>(ruleCondition.Value);
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
                    if (ruleCondition.Operator != EOperator.GreaterThan &&
                        ruleCondition.Operator != EOperator.Equals)
                    {
                        throw new BadHttpRequestException("Đối với điều kiện QuantityOfSpecificProductVariant, chỉ hỗ trợ GreaterThan hoặc Equals.");
                    }
                    QuantityOfSpecificProductVariantModel? model;
                    try
                    {
                        model = JsonSerializer.Deserialize<QuantityOfSpecificProductVariantModel>(ruleCondition.Value);
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
                default:
                    throw new BadHttpRequestException("Loại điều kiện không hợp lệ");
            }
        }

        switch (request.RuleActions.ActionType)
        {
            case EActionType.CartPercentageDiscount:
                if (!decimal.TryParse(request.RuleActions.Value, out var cartPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của CartPercentageDiscount không hợp lệ");
                }

                if (cartPercentageDiscount < 0 || cartPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của CartPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }
                break;
            case EActionType.CartFixedDiscount:
                if(!decimal.TryParse(request.RuleActions.Value, out var cartFixedDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của CartFixedDiscount không hợp lệ");
                }

                if (cartFixedDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của CartFixedDiscount phải lớn hơn hoặc bằng 0.");
                }
                break;
            case EActionType.ItemPercentageDiscount: 
                if(!decimal.TryParse(request.RuleActions.Value, out var itemPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của ItemPercentageDiscount không hợp lệ");
                }

                if (itemPercentageDiscount < 0 || itemPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của ItemPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                break;
            case EActionType.OneItemPercentageDiscount:
                if (!decimal.TryParse(request.RuleActions.Value, out var oneItemPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của OneItemPercentageDiscount không hợp lệ");
                }
                if(oneItemPercentageDiscount < 0 || oneItemPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của OneItemPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Đối với hành động giảm giá một sản phẩm, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
                }
                break;
            case EActionType.ItemFixedAmountDiscount:
                if (!decimal.TryParse(request.RuleActions.Value, out var itemFixedAmountDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của ItemFixedAmountDiscount không hợp lệ");
                }

                if (itemFixedAmountDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của ItemFixedAmountDiscount phải lớn hơn hoặc bằng 0.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                break;
            case EActionType.OneItemFixedAmountDiscount:
                if (!decimal.TryParse(request.RuleActions.Value, out var oneItemFixedAmountDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của OneItemFixedAmountDiscount không hợp lệ");
                }
                if (oneItemFixedAmountDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của OneItemFixedAmountDiscount phải lớn hơn hoặc bằng 0.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                if (request.RuleActions.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Đối với hành động giảm giá một sản phẩm, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
                }
                break;
            // case EActionType.GiveFreeItemSku:
            //     if(!int.TryParse(request.RuleActions.Value, out var freeItemCount))
            //     {
            //         throw new BadHttpRequestException("Giá trị của GiveFreeItemSku không hợp lệ");
            //     }
            //
            //     if (freeItemCount < 0)
            //     {
            //         throw new BadHttpRequestException("Giá trị của GiveFreeItemSku phải lớn hơn hoặc bằng 0.");
            //     }
            //     if (request.RuleActions.TargetCriteriaForItemAction == null)
            //     {
            //         throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
            //     }
            //     if (request.RuleActions.TargetCriteriaForItemAction.Count != 1)
            //     {
            //         throw new BadHttpRequestException("Đối với hành động GiveFreeItemSku, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
            //     }
            //     break;
            default:
                throw new BadHttpRequestException("Loại hành động không hợp lệ");
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
                TargetCriteriaForItemAction = request.RuleActions.TargetCriteriaForItemAction != null ? 
                    JsonSerializer.Serialize(request.RuleActions.TargetCriteriaForItemAction) : null,
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
            Data = promotionRule.Id
        };
    }
}