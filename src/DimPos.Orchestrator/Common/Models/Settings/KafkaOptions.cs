using Confluent.Kafka;

namespace DimPos.Orchestrator.Common.Models.Settings;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string CreateBrandResponse { get; set; } = default!;
    public string CreateBrandAccountRequest { get; set; } = default!;
    public string CreateBrandAccountResponse { get; set; } = default!;
    public string CreateBrandAccountError { get; set; } = default!;
    public string RollbackBrandAccountRequest { get; set; } = default!;
    public string CreateStoreResponse { get; set; } = default!;
    public string CreateStoreAccountRequest { get; set; } = default!;
    public string CreateStoreAccountResponse { get; set; } = default!;
    public string CreateStoreAccountError { get; set; } = default!;
    public string RollbackStoreAccountRequest { get; set; } = default!;
    public string AssignNewStoreMenuResponse { get; set; } = default!;
    public string AddStorePriceRequest { get; set; } = default!;
    public string AddStorePriceResponse { get; set; } = default!;
    public string AddStorePriceError { get; set; } = default!;
    public string RollbackStoreMenuRequest { get; set; } = default!;
    public string RemoveStoreMenuResponse { get; set; } = default!;
    public string RemoveStorePriceRequest { get; set; } = default!;
    public string RemoveStorePriceResponse { get; set; } = default!;
    public string RemoveStorePriceError { get; set; } = default!;
    public string RollbackRemoveStoreMenuRequest { get; set; } = default!;
    public string CreateStaffResponse { get; set; } = default!;
    public string CreateStaffAccountRequest { get; set; } = default!;
    public string CreateStaffAccountResponse { get; set; } = default!;
    public string CreateStaffAccountError { get; set; } = default!;
    public string RollbackStaffStoreAccountRequest { get; set; } = default!;
    public string InternalOrderDoneByStoreResponse { get; set; } = default!;
    public string GetIngredientDetailsRequest { get; set; } = default!;
    public string GetIngredientDetailsResponse { get; set; } = default!;
    public string UpdateInventoryForInternalOrderError { get; set; } = default!;
    public string UpdateInventoryForInternalOrderRequest { get; set; } = default!;
    public string UpdateInventoryForInternalOrderResponse { get; set; } = default!;
    public string ChangeErrorStatusForStorePurchaseOrderRequest { get; set; } = default!;
    public string UpdateBrandMenuItemResponse { get; set; } = default!;
    public string CreateStorePriceForBrandMenuItemRequest { get; set; } = default!;
    public string UpdateStoreByBrandRequest { get; set; } = default!;
    public string UpdateAccountForStoreByBrandRequest { get; set; } = default!;
    public string UpdateAccountForStoreByBrandResponse {get ; set; } = default!;
    public string UpdateAccountForStoreByBrandError { get; set; } = default!;
    public string RollbackUpdateStoreByBrandRequest { get; set; } = default!;
    public string CallbackPaymentResponse { get; set; } = default!;
    public string UpdatePaymentTransactionRequest { get; set; } = default!;
    public string UpdatePaymentTransactionResponse { get; set; } = default!;
    public string UpdateOrderStatusRequest { get; set; } = default!;
    public string UpdateOrderStatusResponse { get; set; } = default!;
    public string UpdateOrderStatusError { get; set; } = default!;
    public string RollbackPaymentTransactionRequest { get; set; } = default!;
    public string CreateOrderResponse { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderRequest { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderResponse { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderError { get; set; } = default!;
    public string UpdateOrderNeedToChangeInventoryRequest { get; set; } = default!;
    public string RollbackInventoryForOrderRequest { get; set; } = default!;
    public string ConfirmForCashOrderResponse { get; set; } = default!;
    public string UpdatePaymentTransactionForCashOrderRequest { get; set; } = default!;
    public string UpdatePaymentTransactionForCashOrderResponse { get; set; } = default!;
    public string UpdatePaymentTransactionForCashOrderError { get; set; } = default!;
    public string RollbackPendingForCashOrderRequest { get; set; } = default!;
}