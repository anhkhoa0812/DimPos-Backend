using System.Text.Json;
using DimPos.Promotion.Application.Common.Protos;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using DimPos.Promotion.Infrastructure.Utils;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.GrpcServices;

public class PromotionGrpcService : Common.Protos.PromotionGrpcService.PromotionGrpcServiceBase
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public PromotionGrpcService(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public override async Task<GetPromotionForOrderResponse> GetPromotionForOrder(GetPromotionForOrderRequest request, ServerCallContext context)
    {
        _logger.Information($"BEGIN: {nameof(GetPromotionForOrder)} - {TimeUtil.GetCurrentSEATime()}");
        
        var promotionRules = await _unitOfWork.GetRepository<PromotionRules>().GetListAsync(
            predicate: x => x.CampaignRuleLinks.Any(x => x.Campaign.IsActive
                                                         && x.Campaign.CampaignStores.Any(cs => cs.StoreId == Guid.Parse(request.StoreId))
                                                         && x.Campaign.StartDate <= TimeUtil.GetCurrentSEATime()
                                                         && x.Campaign.EndDate >= TimeUtil.GetCurrentSEATime())
                            && x.BrandId == Guid.Parse(request.BrandId),
            include: x => x.Include(pr => pr.CampaignRuleLinks)
                .ThenInclude(crl => crl.Campaign)
                .Include(pr => pr.RuleConditions)
                .Include(pr => pr.RuleActions)
        );
        var promotionRuleIds = request.PromotionRuleIds.Select(Guid.Parse).ToList();
        var missingPromotionRuleIds = promotionRuleIds.Except(promotionRules.Select(x => x.Id)).ToList();
        if (missingPromotionRuleIds.Any())
        {
            _logger.Warning("Missing promotion rules: {Join}", string.Join(", ", missingPromotionRuleIds));
            return new GetPromotionForOrderResponse()
            {
                IsSuccess = false,
                ErrorMessage = "Một số mã khuyến mãi không hợp lệ hoặc không tồn tại.",
                Promotions = { new PromotionForOrderResponse() }
            };
        }
        var subtotalAmount = (decimal) request.SubtotalAmount;
        var response = new GetPromotionForOrderResponse();
        response.IsSuccess = true;
        response.ErrorMessage = string.Empty;
        foreach (var promotionRuleId in promotionRuleIds)
        {
            var promotionRule = promotionRules.First(x => x.Id == promotionRuleId);
            var appliedPromotionDetail = new PromotionForOrderResponse()
            {
                Id = promotionRule.Id.ToString(),
                Name = promotionRule.Name,
                Description = promotionRule.Description ?? String.Empty,
                OrderItemFree = null,
                IsGiveFreeItemSku = false,
                DiscountAmountApplied = 0
            };
            foreach (var conditionRule in promotionRule.RuleConditions)
            {
                switch (conditionRule.ConditionType)
                {
                    case EConditionType.MinCartValue:
                        var conditionValueDecimal = Decimal.Parse(conditionRule.Value);
                        if (conditionRule.Operator == EOperator.GreaterThanOrEqual)
                        {
                            if (subtotalAmount < conditionValueDecimal)
                            {
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Giá trị giỏ hàng hiện tại ({subtotalAmount}) không đủ điều kiện tối thiểu ({conditionValueDecimal}) cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            }
                        }
                        else if (conditionRule.Operator == EOperator.GreaterThan)
                        {
                            if (subtotalAmount <= conditionValueDecimal)
                            {
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Giá trị giỏ hàng hiện tại ({subtotalAmount}) không đủ điều kiện tối thiểu ({conditionValueDecimal}) cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };

                            }
                        }
                        else
                        {
                            return new GetPromotionForOrderResponse()
                            {
                                IsSuccess = false,
                                ErrorMessage =
                                    $"Toán tử không hợp lệ: {conditionRule.Operator} cho điều kiện {conditionRule.ConditionType} trong mã khuyến mãi {promotionRule.Name}",
                                Promotions = { new PromotionForOrderResponse() }
                            };
                        } 
                        break;
                    case EConditionType.CartContainsProductVariant: 
                        List<Guid>? productVariantIds = JsonSerializer.Deserialize<List<Guid>>(conditionRule.Value); 
                        if(productVariantIds == null) 
                            throw new BadHttpRequestException("Không tìm thấy danh sách sản phẩm để áp dụng khuyến mãi"); 
                        if (conditionRule.Operator == EOperator.ContainsAnyInList) 
                        { 
                            if (!request.OrderItems.Any(ci => productVariantIds.Contains(Guid.Parse(ci.ProductVariantId)))) 
                            { 
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Giỏ hàng không chứa bất kỳ sản phẩm nào trong danh sách: {string.Join(", ", productVariantIds)} cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            } 
                        }
                        else if (conditionRule.Operator == EOperator.ContainsAllInList) 
                        { 
                            if (!productVariantIds.All(pvId => request.OrderItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId))) 
                            { 
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage = 
                                        $"Giỏ hàng không chứa tất cả sản phẩm trong danh sách: {string.Join(", ", productVariantIds)} cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            } 
                        }
                        else if(conditionRule.Operator == EOperator.ContainsExactList) 
                        {
                            if (request.OrderItems.Count != productVariantIds.Count || 
                                !productVariantIds.All(pvId => request.OrderItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId)))
                            {
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Giỏ hàng không chứa đúng danh sách sản phẩm: {string.Join(", ", productVariantIds)} cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            }
                        }
                        else 
                        { 
                            return new GetPromotionForOrderResponse()
                            {
                                IsSuccess = false,
                                ErrorMessage =
                                    $"Toán tử không hợp lệ: {conditionRule.Operator} cho điều kiện {conditionRule.ConditionType} trong mã khuyến mãi {promotionRule.Name}",
                                Promotions = { new PromotionForOrderResponse() }
                            };
                        }
                        break;
                    case EConditionType.QuantityOfSpecificProductVariant: 
                        var quantityOfSpecificProductVariantModel = 
                            JsonSerializer.Deserialize<QuantityOfSpecificProductVariantModel>(conditionRule.Value); 
                        if (conditionRule.Operator == EOperator.GreaterThanOrEqual) 
                        { 
                            if(!request.OrderItems.Any(ci => Guid.Parse(ci.ProductVariantId) == quantityOfSpecificProductVariantModel.ProductVariantId 
                                                             && ci.Quantity >= quantityOfSpecificProductVariantModel.Quantity)) 
                            { 
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Số lượng sản phẩm cụ thể trong giỏ hàng không đủ: {quantityOfSpecificProductVariantModel.ProductVariantId}, Số lượng yêu cầu: {quantityOfSpecificProductVariantModel.Quantity} cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            } 
                        }
                        else if (conditionRule.Operator == EOperator.Equals) 
                        { 
                            if (!request.OrderItems.Any(oi =>
                                    Guid.Parse(oi.ProductVariantId) == quantityOfSpecificProductVariantModel.ProductVariantId
                                    && oi.Quantity == quantityOfSpecificProductVariantModel.Quantity)) 
                            { 
                                return new GetPromotionForOrderResponse()
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Số lượng sản phẩm cụ thể trong giỏ hàng không đúng: {quantityOfSpecificProductVariantModel.ProductVariantId}, Số lượng yêu cầu: {quantityOfSpecificProductVariantModel.Quantity} cho mã khuyến mãi {promotionRule.Name}",
                                    Promotions = { new PromotionForOrderResponse() }
                                };
                            } 
                        }
                        else 
                        {
                            return new GetPromotionForOrderResponse()
                            {
                                IsSuccess = false,
                                ErrorMessage =
                                    $"Toán tử không hợp lệ: {conditionRule.Operator} cho điều kiện {conditionRule.ConditionType} trong mã khuyến mãi {promotionRule.Name}",
                                Promotions = { new PromotionForOrderResponse() }
                            };
                        } 
                        break; 
                    default:
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"Điều kiện không hợp lệ: {conditionRule.ConditionType} trong mã khuyến mãi {promotionRule.Name}",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                }
            }

            switch (promotionRule.RuleActions.ActionType)
            {
                case EActionType.CartPercentageDiscount:
                    var percentageDiscount = Decimal.Parse(promotionRule.RuleActions.Value);
                    if (percentageDiscount < 0 || percentageDiscount > 100)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Giá trị giảm giá phần trăm không hợp lệ",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    appliedPromotionDetail.DiscountAmountApplied = (float) (subtotalAmount * (percentageDiscount / 100));
                    if( (decimal) appliedPromotionDetail.DiscountAmountApplied > promotionRule.RuleActions.MaxDiscountAmountForPercentage)
                    {
                        appliedPromotionDetail.DiscountAmountApplied =
                            (float)promotionRule.RuleActions.MaxDiscountAmountForPercentage;
                    }
                    break;
                case EActionType.CartFixedDiscount:
                    var fixedDiscount = Decimal.Parse(promotionRule.RuleActions.Value);
                    if (fixedDiscount < 0)
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Giá trị giảm giá cố định không hợp lệ",
                            Promotions = { new PromotionForOrderResponse() }
                        };

                    appliedPromotionDetail.DiscountAmountApplied = (float) fixedDiscount;
                    break;
                case EActionType.ItemPercentageDiscount: 
                    var percentageItemDiscount = Decimal.Parse(promotionRule.RuleActions.Value);
                    if(percentageItemDiscount < 0 || percentageItemDiscount > 100)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Giá trị giảm giá phần trăm cho sản phẩm không hợp lệ",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }

                    if (promotionRule.RuleActions.TargetCriteriaForItemAction == null)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Không tìm thấy sản phẩm để áp dụng giảm giá phần trăm",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    var targetCriteriaForItemActionForItemPercentage =
                        JsonSerializer.Deserialize<List<Guid>>(promotionRule.RuleActions.TargetCriteriaForItemAction);
                    if (targetCriteriaForItemActionForItemPercentage == null)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Không tìm thấy danh sách sản phẩm để áp dụng giảm giá phần trăm",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    
                    var itemForActionList = request.OrderItems
                        .Where(ci => targetCriteriaForItemActionForItemPercentage.Contains(Guid.Parse(ci.ProductVariantId)))
                        .ToList();
                    foreach (var itemForAction in itemForActionList)
                    {
                        var itemDiscountAmount = itemForAction.UnitPrice * itemForAction.Quantity * (float) (percentageItemDiscount / 100);
                        appliedPromotionDetail.DiscountAmountApplied += itemDiscountAmount * itemForAction.Quantity;
                    }
                    break;
                case EActionType.OneItemPercentageDiscount: 
                    var percentageFixedItemDiscount = Decimal.Parse(promotionRule.RuleActions.Value);
                    if(percentageFixedItemDiscount < 0 || percentageFixedItemDiscount > 100)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Giá trị giảm giá phần trăm cố định cho sản phẩm không hợp lệ",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }

                    if (promotionRule.RuleActions.TargetCriteriaForItemAction == null)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Không tìm thấy sản phẩm để áp dụng giảm giá phần trăm cố định",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    List<Guid>? targetCriteriaForItemActionForOneItemPercentage =
                        JsonSerializer.Deserialize<List<Guid>>(
                            promotionRule.RuleActions.TargetCriteriaForItemAction);

                    if (targetCriteriaForItemActionForOneItemPercentage == null || targetCriteriaForItemActionForOneItemPercentage.Count != 1)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Chỉ có thể áp dụng giảm giá phần trăm cố định cho một sản phẩm",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                
                    var targetOrderItem = request.OrderItems
                        .FirstOrDefault(ci => Guid.Parse(ci.ProductVariantId) == targetCriteriaForItemActionForOneItemPercentage.FirstOrDefault());
                    appliedPromotionDetail.DiscountAmountApplied = targetOrderItem.UnitPrice * targetOrderItem.Quantity 
                        * (float) (percentageFixedItemDiscount / 100);
                    break;
                case EActionType.ItemFixedAmountDiscount:
                    var amountDiscount = Decimal.Parse(promotionRule.RuleActions.Value);
                    if (amountDiscount < 0)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Giá trị giảm giá cố định cho sản phẩm không hợp lệ",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    if (promotionRule.RuleActions.TargetCriteriaForItemAction == null)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Không tìm thấy sản phẩm để áp dụng giảm giá cố định",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    List<Guid>? targetCriteriaForItemActionForItemFixed =
                        JsonSerializer.Deserialize<List<Guid>>(promotionRule.RuleActions.TargetCriteriaForItemAction);
                    if (targetCriteriaForItemActionForItemFixed == null)
                    {
                        return new GetPromotionForOrderResponse()
                        {
                            IsSuccess = false,
                            ErrorMessage = "Không tìm thấy danh sách sản phẩm để áp dụng giảm giá cố định",
                            Promotions = { new PromotionForOrderResponse() }
                        };
                    }
                    var orderItemForFixedActionList = request.OrderItems
                        .Where(ci => targetCriteriaForItemActionForItemFixed.Contains(Guid.Parse(ci.ProductVariantId)))
                        .ToList();
                    foreach (var orderItemForFixedAction in orderItemForFixedActionList)
                    {
                        var orderItemForFixedActionFixed = amountDiscount;
                        appliedPromotionDetail.DiscountAmountApplied += (float) orderItemForFixedActionFixed * orderItemForFixedAction.Quantity;
                    }
                    break;
            case EActionType.OneItemFixedAmountDiscount:
                var amountDiscountForOne = Decimal.Parse(promotionRule.RuleActions.Value);
                if(amountDiscountForOne < 0)
                {
                    return new GetPromotionForOrderResponse()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Giá trị giảm giá cố định cho một sản phẩm không hợp lệ",
                        Promotions = { new PromotionForOrderResponse() }
                    };
                }
                if(promotionRule.RuleActions.TargetCriteriaForItemAction == null)
                {
                    break;
                }
                var targetCriteriaForItemActionForOneItemFixed =
                    JsonSerializer.Deserialize<List<Guid>>(promotionRule.RuleActions.TargetCriteriaForItemAction);
                if(targetCriteriaForItemActionForOneItemFixed == null || targetCriteriaForItemActionForOneItemFixed.Count != 1)
                {
                    return new GetPromotionForOrderResponse()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Chỉ có thể áp dụng giảm giá cố định cho một sản phẩm",
                        Promotions = { new PromotionForOrderResponse() }
                    };
                }
                var targetOrderItemForFixed = request.OrderItems
                    .FirstOrDefault(ci => Guid.Parse(ci.ProductVariantId) == targetCriteriaForItemActionForOneItemFixed.FirstOrDefault());
                if (targetOrderItemForFixed == null)
                {
                    return new GetPromotionForOrderResponse()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Không tìm thấy sản phẩm để áp dụng giảm giá cố định",
                        Promotions = { new PromotionForOrderResponse() }
                    };
                }
                appliedPromotionDetail.DiscountAmountApplied = (float) amountDiscountForOne;
                break;
            // case EActionType.GiveFreeItemSku:
            //     var quantityFree = int.Parse(promotionRule.RuleActions.Value);
            //     if (quantityFree < 0)
            //     {
            //         return new GetPromotionForOrderResponse()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "Số lượng sản phẩm miễn phí không hợp lệ",
            //             Promotions = { new PromotionForOrderResponse() }
            //         };
            //     }
            //
            //     if (promotionRule.RuleActions.TargetCriteriaForItemAction == null)
            //     {
            //         return new GetPromotionForOrderResponse()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "Không tìm thấy sản phẩm để áp dụng miễn phí",
            //             Promotions = { new PromotionForOrderResponse() }
            //         };
            //     }
            //     var targetCriteriaForItemActionForFree =
            //         JsonSerializer.Deserialize<List<Guid>>(promotionRule.RuleActions.TargetCriteriaForItemAction);
            //     if(targetCriteriaForItemActionForFree == null || targetCriteriaForItemActionForFree.Count != 1)
            //     {
            //         return new GetPromotionForOrderResponse()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "Chỉ có thể áp dụng sản phẩm miễn phí cho một sản phẩm",
            //             Promotions = { new PromotionForOrderResponse() }
            //         };
            //     }
            //     var targetOrderItemForFree = request.OrderItems
            //         .FirstOrDefault(ci => targetCriteriaForItemActionForFree.Contains(Guid.Parse(ci.ProductVariantId)));
            //     if (targetOrderItemForFree == null)
            //     {
            //         return new GetPromotionForOrderResponse()
            //         {
            //             IsSuccess = false,
            //             ErrorMessage = "Không tìm thấy sản phẩm để áp dụng miễn phí",
            //             Promotions = { new PromotionForOrderResponse() }
            //         };
            //     }
            //     
            //     targetOrderItemForFree.Quantity += quantityFree;
            //     appliedPromotionDetail.DiscountAmountApplied += targetOrderItemForFree.UnitPrice * quantityFree;
            //     appliedPromotionDetail.IsGiveFreeItemSku = true;
            //     appliedPromotionDetail.OrderItemFree = targetOrderItemForFree;
            //     
            //     break;
            }
            response.Promotions.Add(appliedPromotionDetail);
        }
        
        _logger.Information($"END: {nameof(GetPromotionForOrder)} - {TimeUtil.GetCurrentSEATime()}");
        return response;
    }
}