using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models.Response;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? PosDeviceId { get; set; }
    public Guid? StaffAccountIdCreating { get; set; }
    public Guid? CustomerIdLinked { get; set; }
    public EServiceMethod ServiceMethod { get; set; }
    public int? TakeNumberDineIn { get; set; }
    public DateTime? PickupTimeRequested { get; set; }
    public ECartStatus Status { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TotalItemDiscountAmount { get; set; }
    public decimal OrderLevelDiscountAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal FinalTotalAmount { get; set; }
    public string? CustomerNotesForOrder { get; set; }
    public string? StaffNotesForOrder { get; set; }
    public Guid? SelectedPaymentMethodConfigId { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantityOfItems { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public List<CartItemResponse>? CartItems { get; set; } = new List<CartItemResponse>();
    public List<PromotionResponse>? PromotionsApplied { get; set; } = new List<PromotionResponse>();
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public string? ProductImageUrlSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public decimal ItemSubtotalAmount { get; set; }
    public string? NotesForItem { get; set; }
    public DateTime AddedAt { get; set; }
    public List<ModifierGroupItemResponse>? ModifierGroupItems { get; set; }
    public List<ProductExtraItemResponse>? ExtraItems { get; set; }
}

public class ModifierGroupItemResponse
{
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
    public Guid? RelatedComboProductVariantItemId { get; set; }
    public string? RelatedComboProductVariantItemName { get; set; }
}

public class ProductExtraItemResponse
{
    public Guid ExtraProductVariantId { get; set; }
    public string ExtraProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public Guid? RelatedProductVariantId { get; set; }
}
public class PromotionResponse
{
    public Guid Id { get; set; }
    public Guid PromotionRuleId { get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public decimal DiscountValueCalculated { get; set; }
    public List<Guid>? ApplicableCartItemIds { get; set; }
    public List<ConditionRuleResponse> ConditionRules { get; set; }
    public EActionType ActionType { get; set; }
    public string ActionValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal? MaxDiscountAmountForPercentage { get; set; }
    
}
public class ConditionRuleResponse
{
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
}