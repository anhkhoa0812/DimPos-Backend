using System.Text.Json;
using DimPos.Basket.Application.Common.Protos;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using DimPos.Promotion.Domain.Models.PromotionRules;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Mediator;

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
            predicate: x => x.CampaignRuleLinks.Any(x => x.Campaign.Status == ECampaignsStatus.Active
                            && x.Campaign.CampaignStores.Any(cs => cs.StoreId == storeId))
            && x.BrandId == Guid.Parse(cart.BrandId)
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
                    Description = promotionRule.Description,
                    Priority = promotionRule.Priority,
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
                                   if((decimal) cart.SubtotalAmount < conditionValueDecimal) 
                                        promotionRuleResponse.IsValid = false;
                                }
                                else if (conditionRule.Operator == EOperator.GreaterThan)
                                {
                                   if((decimal) cart.SubtotalAmount <= conditionValueDecimal) 
                                       promotionRuleResponse.IsValid = false;
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
                                    } 
                                }
                                else if (conditionRule.Operator == EOperator.ContainsAllInList) 
                                { 
                                    if (!productVariantIds.All(pvId => cart.CartItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId))) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                    } 
                                }
                                else if(conditionRule.Operator == EOperator.ContainsExactList) 
                                {
                                    if (cart.CartItems.Count != productVariantIds.Count || 
                                        !productVariantIds.All(pvId => cart.CartItems.Any(ci => Guid.Parse(ci.ProductVariantId) == pvId)))
                                    {
                                        promotionRuleResponse.IsValid = false;
                                    }
                                }
                                else
                                {
                                    promotionRuleResponse.IsValid = false;
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
                                    } 
                                }
                                else if (conditionRule.Operator == EOperator.Equals) 
                                { 
                                    if (!cart.CartItems.Any(ci =>
                                          Guid.Parse(ci.ProductVariantId) == quantityOfSpecificProductVariantModel.ProductVariantId
                                          && ci.Quantity == quantityOfSpecificProductVariantModel.Quantity)) 
                                    { 
                                        promotionRuleResponse.IsValid = false;
                                    } 
                                }
                                else 
                                { 
                                    promotionRuleResponse.IsValid = false;
                                } 
                                break; 
                            default: 
                                promotionRuleResponse.IsValid = false; 
                                break;
                        } 
                    }
                }
                promotionRuleListResponse.Add(promotionRuleResponse);
            }
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = promotionRuleListResponse
        };
        
    }
}