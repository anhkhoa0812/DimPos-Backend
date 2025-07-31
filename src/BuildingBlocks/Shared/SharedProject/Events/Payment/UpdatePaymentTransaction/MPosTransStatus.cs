namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public enum MPosTransStatus
{
    Approved = 100,
    Reversed = 101,
    Voided = 102,
    PendingSignature = 103,
    Settled = 104,
    Pending = 90,
    Rejected = 91,
    Refunded = 99,
    Fail = 97
}