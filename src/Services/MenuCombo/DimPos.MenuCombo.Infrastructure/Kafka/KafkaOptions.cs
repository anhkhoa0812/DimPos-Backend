using Confluent.Kafka;

namespace DimPos.MenuCombo.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string AssignNewStoreMenuResponse { get; set; } = default!;
    public string RollbackStoreMenuRequest { get; set; } = default!;
    public string RemoveStoreMenuResponse { get; set; } = default!;
    public string RollbackRemoveStoreMenuRequest { get; set; } = default!;
}