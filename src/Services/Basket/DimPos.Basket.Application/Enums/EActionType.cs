namespace DimPos.Basket.Application.Enums;

public enum EActionType
{
    CartPercentageDiscount = 0,
    CartFixedDiscount = 1,
    ItemPercentageDiscount = 2,
    ItemFixedAmountDiscount = 3,
    GiveFreeItemSku = 4,
    ApplyBundlePrice = 5,
    IssueVoucherFromBatch = 6,
    UpgradeCustomerLoyaltyTier = 7
}