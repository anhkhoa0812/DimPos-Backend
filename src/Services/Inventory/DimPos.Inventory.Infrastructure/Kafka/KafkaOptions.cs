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
}