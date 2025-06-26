namespace DimPos.Order.Domain.Enums;

public enum EOrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Preparing = 2,
    ReadyForPickup = 3,
    Completed = 4,
    Cancelled = 5,
}