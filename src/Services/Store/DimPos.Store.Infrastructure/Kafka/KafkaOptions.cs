using Confluent.Kafka;

namespace DimPos.Store.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string CreateStoreResponse { get; set; } = default!;
    public string RollbackStoreAccountRequest { get; set; } = default!;
}