using Confluent.Kafka;

namespace DimPos.Inventory.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string UpdateInventoryForInternalOrderRequest { get; set; } = default!;
    public string UpdateInventoryForInternalOrderError { get; set; } = default!;
    public string UpdateInventoryForInternalOrderResponse { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderRequest { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderResponse { get; set; } = default!;
    public string UpdateInventoryForSuccessOrderError { get; set; } = default!;
    public string RollbackInventoryForOrderRequest { get; set; } = default!;
    public string UpdateInventoryForCancelOrderRequest { get; set; } = default!;
    public string UpdateInventoryForCancelOrderResponse { get; set; } = default!;
    public string UpdateInventoryForCancelOrderError { get; set; } = default!;
    public string ChangeIsNeedToUpdateInventoryForOrderRequest { get; set; } = default!;
    public string SendNotificationForMultipleAccountRequest { get; set; } = default!;
}