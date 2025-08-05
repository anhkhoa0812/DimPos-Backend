namespace DimPos.Order.Domain.Enums;

public enum EOrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    ReadyForPickup = 2,
    Completed = 3,
    Cancelled = 4,
}