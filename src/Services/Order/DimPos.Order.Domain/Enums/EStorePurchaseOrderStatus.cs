namespace DimPos.Order.Domain.Enums;

public enum EStorePurchaseOrderStatus
{
    New = 0,
    BrandConfirmed = 1,
    RejectedByBrand = 2,
    DoneByStore = 3,
    CancelledByStore = 4,
    CancelledByBrand = 5,
}