using System.Text.Json;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.RuleAction.Command.UpdateRuleAction;

public class UpdateRuleActionCommandHandler : IRequestHandler<UpdateRuleActionCommand, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateRuleActionCommandHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateRuleActionCommand request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu");

        var ruleAction = await _unitOfWork.GetRepository<RuleActions>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.RuleActionId
            && x.PromotionRuleId == request.PromotionRuleId 
            && x.PromotionRule.BrandId == brandId,
            include: x => x.Include(x => x.PromotionRule)
        );
        if (ruleAction == null)
            throw new BadHttpRequestException("Không tìm thấy hành động quy tắc khuyến mãi với ID đã cung cấp.");
        switch (request.ActionType)
        {
            case EActionType.CartPercentageDiscount:
                if (!decimal.TryParse(request.Value, out var cartPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của CartPercentageDiscount không hợp lệ");
                }

                if (cartPercentageDiscount < 0 || cartPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của CartPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }

                if (request.MaxDiscountAmountForPercentage != null)
                {
                    if(request.MaxDiscountAmountForPercentage < 0)
                        throw new BadHttpRequestException("Giá trị của MaxDiscountAmountForPercentage phải lớn hơn hoặc bằng 0.");
                }
                ruleAction.MaxDiscountAmountForPercentage = request.MaxDiscountAmountForPercentage;
                break;
            case EActionType.CartFixedDiscount:
                if(!decimal.TryParse(request.Value, out var cartFixedDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của CartFixedDiscount không hợp lệ");
                }

                if (cartFixedDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của CartFixedDiscount phải lớn hơn hoặc bằng 0.");
                }
                break;
            case EActionType.ItemPercentageDiscount: 
                if(!decimal.TryParse(request.Value, out var itemPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của ItemPercentageDiscount không hợp lệ");
                }

                if (itemPercentageDiscount < 0 || itemPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của ItemPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }

                ruleAction.TargetCriteriaForItemAction = JsonSerializer.Serialize(request.TargetCriteriaForItemAction);
                break;
            case EActionType.OneItemPercentageDiscount:
                if (!decimal.TryParse(request.Value, out var oneItemPercentageDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của OneItemPercentageDiscount không hợp lệ");
                }
                if(oneItemPercentageDiscount < 0 || oneItemPercentageDiscount > 100)
                {
                    throw new BadHttpRequestException("Giá trị của OneItemPercentageDiscount phải nằm trong khoảng từ 0 đến 100.");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                if (request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Đối với hành động giảm giá một sản phẩm, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
                }
                ruleAction.TargetCriteriaForItemAction = JsonSerializer.Serialize(request.TargetCriteriaForItemAction);
                break;
            case EActionType.ItemFixedAmountDiscount:
                if (!decimal.TryParse(request.Value, out var itemFixedAmountDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của ItemFixedAmountDiscount không hợp lệ");
                }

                if (itemFixedAmountDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của ItemFixedAmountDiscount phải lớn hơn hoặc bằng 0.");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                ruleAction.TargetCriteriaForItemAction = JsonSerializer.Serialize(request.TargetCriteriaForItemAction);
                break;
            case EActionType.OneItemFixedAmountDiscount:
                if (!decimal.TryParse(request.Value, out var oneItemFixedAmountDiscount))
                {
                    throw new BadHttpRequestException("Giá trị của OneItemFixedAmountDiscount không hợp lệ");
                }
                if (oneItemFixedAmountDiscount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của OneItemFixedAmountDiscount phải lớn hơn hoặc bằng 0.");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                if (request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Đối với hành động giảm giá một sản phẩm, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
                }
                ruleAction.TargetCriteriaForItemAction = JsonSerializer.Serialize(request.TargetCriteriaForItemAction);
                break;
            case EActionType.GiveFreeItemSku:
                if(!int.TryParse(request.Value, out var freeItemCount))
                {
                    throw new BadHttpRequestException("Giá trị của GiveFreeItemSku không hợp lệ");
                }

                if (freeItemCount < 0)
                {
                    throw new BadHttpRequestException("Giá trị của GiveFreeItemSku phải lớn hơn hoặc bằng 0.");
                }
                if (request.TargetCriteriaForItemAction == null)
                {
                    throw new BadHttpRequestException("Đối với các hành động liên quan đến sản phẩm, cần cung cấp TargetCriteriaForItemAction.");
                }
                if (request.TargetCriteriaForItemAction.Count != 1)
                {
                    throw new BadHttpRequestException("Đối với hành động GiveFreeItemSku, TargetCriteriaForItemAction chỉ có thể chứa một sản phẩm.");
                }
                ruleAction.TargetCriteriaForItemAction = JsonSerializer.Serialize(request.TargetCriteriaForItemAction);
                break;
            default:
                throw new BadHttpRequestException("Loại hành động không hợp lệ");
        }
        ruleAction.ActionType = request.ActionType;
        ruleAction.Value = request.Value;
        _unitOfWork.GetRepository<RuleActions>().UpdateAsync(ruleAction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new Exception("Cập nhật hành động quy tắc khuyến mãi không thành công");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật hành động quy tắc khuyến mãi thành công",
            Data = ruleAction.Id
        };
    }
}