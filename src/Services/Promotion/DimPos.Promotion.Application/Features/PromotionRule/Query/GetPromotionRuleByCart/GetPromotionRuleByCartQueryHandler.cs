using System.Text.Json;
using DimPos.Basket.Application.Common.Protos;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using DimPos.Promotion.Infrastructure.Utils;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleByCart;

public class GetPromotionRuleByCartQueryHandler : IRequestHandler<GetPromotionRuleByCartQuery, ApiResponse>
{
    private readonly IUnitOfWork<PromotionContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly BasketGrpcService.BasketGrpcServiceClient _basketGrpcService;
    
    public GetPromotionRuleByCartQueryHandler(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger, 
        IClaimService claimService, BasketGrpcService.BasketGrpcServiceClient basketGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _basketGrpcService = basketGrpcService ?? throw new ArgumentNullException(nameof(basketGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetPromotionRuleByCartQuery request, CancellationToken cancellationToken)
    {
        var staffAccountId = _claimService.GetCurrentUserId;
        if (staffAccountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản nhân viên");
        }
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng");
        }
        
        var cart = await _basketGrpcService.GetCartByCartIdAsync(new GetCartByCartIdRequest()
        {
            CartId = request.CartId.ToString(),
            StaffAccountId = staffAccountId.ToString()
        }, cancellationToken: cancellationToken);
        if (cart == null)
        {
            throw new BadHttpRequestException("Giỏ hàng không tồn tại hoặc đã bị xóa.");
        }

        var promotionRules = await _unitOfWork.GetRepository<PromotionRules>().GetListAsync(
            predicate: x => x.CampaignRuleLinks.Any(x => x.Campaign.IsActive
                            && x.Campaign.CampaignStores.Any(cs => cs.StoreId == storeId)
                            && x.Campaign.StartDate <= TimeUtil.GetCurrentSEATime()
                            && x.Campaign.EndDate >= TimeUtil.GetCurrentSEATime())
            && x.BrandId == Guid.Parse(cart.BrandId),
            include: x => x.Include(pr => pr.CampaignRuleLinks)
                .ThenInclude(crl => crl.Campaign)
                .Include(pr => pr.RuleConditions)
                .Include(pr => pr.RuleActions)
        );
        
        var promotionRuleListResponse = new List<PromotionRulesByCartResponse>();
        
        if (promotionRules.Any())
        {
            foreach (var promotionRule in promotionRules)
            {
                var promotionRuleResponse = new PromotionRulesByCartResponse()
                {
                    Id = promotionRule.Id,
                    Name = promotionRule.Name,
                    ShortDescription = promotionRule.ShortDescription,
                    Description = promotionRule.Description,
                    Priority = promotionRule.Priority,
                    IsValid = true,
                    RuleConditions = promotionRule.RuleConditions.Select(x => new RuleConditionsForCartResponse()
                    {
                        Id = x.Id,
                        ConditionType = x.ConditionType,
                        Operator = x.Operator,
                        Value = x.Value
                    }).ToList(),
                    RuleAction = new RuleActionsForCartResponse()
                    {
                        Id = promotionRule.RuleActions.Id,
                        ActionType = promotionRule.RuleActions.ActionType,
                        Value = promotionRule.RuleActions.Value,
                        TargetCriteriaForItemAction = !string.IsNullOrEmpty(promotionRule.RuleActions.TargetCriteriaForItemAction)
                            ? JsonSerializer.Deserialize<List<Guid>>(promotionRule.RuleActions.TargetCriteriaForItemAction) 
                            : null,
                        MaxDiscountAmountForPercentage = promotionRule.RuleActions.MaxDiscountAmountForPercentage
                    }
                };
                if (!promotionRule.IsActive)
                {
                    promotionRuleResponse.IsValid = false;
                }
                else
                {
                    foreach (var conditionRule  in promotionRule.RuleConditions) 
                    { 
                        switch (conditionRule.ConditionType) 
                        { 
                            case EConditionType.MinCartValue:
                                var conditionValueDecimal = Decimal.Parse(conditionRule.Value);
                                if (conditionRule.Operator == EOperator.GreaterThanOrEqual)
                                {
                                    if ((decimal)cart.SubtotalAmount < conditionValueDecimal)
                                    {
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Giá trị giỏ hàng hiện tại: {CurrentValue}, Giá trị điều kiện: {ConditionValue}, Toán tử: {Operator}",
                                            cart.SubtotalAmount, conditionValueDecimal, conditionRule.Operator);
                                    }
                                }
                                else if (conditionRule.Operator == EOperator.GreaterThan)
                                {
                                    if ((decimal)cart.SubtotalAmount <= conditionValueDecimal)
                                    {
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Giá trị giỏ hàng hiện tại: {CurrentValue}, Giá trị điều kiện: {ConditionValue}, Toán tử: {Operator}, Id cua PromotionRule: {PromotionRuleId}",
                                            cart.SubtotalAmount, conditionValueDecimal, conditionRule.Operator, promotionRule.Id);
                                    }
                                }
                                else
                                {
                                    promotionRuleResponse.IsValid = false;
                                } 
                                break;
                            case EConditionType.CartContainsProductVariant: 
                                List<Guid>? productVariantIds = JsonSerializer.Deserialize<List<Guid>>(conditionRule.Value); 
                                if(productVariantIds == null) 
                                    throw new BadHttpRequestException("Không tìm thấy danh sách sản phẩm để áp dụng khuyến mãi"); 
                                if (conditionRule.Operator == EOperator.ContainsAnyInList) 
                                { 
                                    if (!cart.CartItems.Any(ci => productVariantIds.Contains(Guid.Parse(ci.ProductVariantId)))) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Giỏ hàng không chứa bất kỳ sản phẩm nào trong danh sách: {ProductVariantIds}, Id cua PromotionRule: {PromotionRuleId}", 
                                            productVariantIds, promotionRule.Id);
                                    } 
                                }
                                else if (conditionRule.Operator == EOperator.ContainsAllInList) 
                                { 
                                    if (!productVariantIds.All(pvId => cart.CartItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId))) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Giỏ hàng không chứa tất cả sản phẩm trong danh sách: {ProductVariantIds}, Id cua PromotionRule: {PromotionRuleId}", 
                                            productVariantIds, promotionRule.Id);
                                    } 
                                }
                                else if(conditionRule.Operator == EOperator.ContainsExactList) 
                                {
                                    if (cart.CartItems.Count != productVariantIds.Count || 
                                        !productVariantIds.All(pvId => cart.CartItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId)))
                                    {
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Giỏ hàng không chứa đúng danh sách sản phẩm: {ProductVariantIds}, Id cua PromotionRule: {PromotionRuleId}", 
                                            productVariantIds, promotionRule.Id);
                                    }
                                }
                                else
                                {
                                    promotionRuleResponse.IsValid = false;
                                    _logger.Information("Toán tử không hợp lệ cho điều kiện chứa sản phẩm trong giỏ hàng: {Operator}, Id cua PromotionRule: {PromotionRuleId}", 
                                        conditionRule.Operator, promotionRule.Id);
                                }
                                break; 
                            case EConditionType.QuantityOfSpecificProductVariant: 
                                var quantityOfSpecificProductVariantModel = 
                                    JsonSerializer.Deserialize<QuantityOfSpecificProductVariantModel>(conditionRule.Value); 
                                if (conditionRule.Operator == EOperator.GreaterThanOrEqual) 
                                { 
                                    if(!cart.CartItems.Any(ci => Guid.Parse(ci.ProductVariantId) == quantityOfSpecificProductVariantModel.ProductVariantId 
                                                               && ci.Quantity >= quantityOfSpecificProductVariantModel.Quantity)) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Số lượng sản phẩm cụ thể trong giỏ hàng không đủ: {ProductVariantId}, Số lượng yêu cầu: {RequiredQuantity}, Id cua PromotionRule: {PromotionRuleId}",
                                            quantityOfSpecificProductVariantModel.ProductVariantId, quantityOfSpecificProductVariantModel.Quantity, promotionRule.Id);
                                    } 
                                }
                                else if (conditionRule.Operator == EOperator.Equals) 
                                { 
                                    if (!cart.CartItems.Any(ci =>
                                          Guid.Parse(ci.ProductVariantId) == quantityOfSpecificProductVariantModel.ProductVariantId
                                          && ci.Quantity == quantityOfSpecificProductVariantModel.Quantity)) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                        _logger.Information("Số lượng sản phẩm cụ thể trong giỏ hàng không đúng: {ProductVariantId}, Số lượng yêu cầu: {RequiredQuantity}, Id cua PromotionRule: {PromotionRuleId}",
                                            quantityOfSpecificProductVariantModel.ProductVariantId, quantityOfSpecificProductVariantModel.Quantity, promotionRule.Id);
                                    } 
                                }
                                else 
                                { 
                                    promotionRuleResponse.IsValid = false;
                                    _logger.Information("Toán tử không hợp lệ cho điều kiện số lượng sản phẩm cụ thể: {Operator}, Id cua PromotionRule: {PromotionRuleId}", 
                                        conditionRule.Operator, promotionRule.Id);
                                } 
                                break; 
                            default: 
                                promotionRuleResponse.IsValid = false; 
                                _logger.Information("Loại điều kiện không hợp lệ: {ConditionType}, Id cua PromotionRule: {PromotionRuleId}", 
                                    conditionRule.ConditionType, promotionRule.Id);
                                break;
                        } 
                    }
                }
                promotionRuleListResponse.Add(promotionRuleResponse);
            }
        }

        var response = promotionRuleListResponse.OrderByDescending(x => x.IsValid).ToList();
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
        
    }
}