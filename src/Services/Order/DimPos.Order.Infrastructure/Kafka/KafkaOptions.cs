using Confluent.Kafka;

namespace DimPos.Order.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string InternalOrderDoneByStoreResponse { get; set; } = default!;
    public string ChangeErrorStatusForStorePurchaseOrderRequest { get; set; } = default!;
    public string UpdateOrderStatusRequest { get; set; } = default!;
    public string UpdateOrderStatusResponse { get; set; } = default!;
    public string UpdateOrderStatusError { get; set; } = default!;
    public string CreateOrderResponse { get; set; } = default!;
    public string UpdateOrderNeedToChangeInventoryRequest { get; set; } = default!;
}