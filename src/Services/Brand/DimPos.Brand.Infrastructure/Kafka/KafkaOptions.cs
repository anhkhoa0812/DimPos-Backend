using Confluent.Kafka;

namespace DimPos.Brand.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string CreateBrandResponse { get; set; } = default!;
    public string RollbackBrandAccountRequest { get; set; } = default!;
    public string UpdateBrandPasswordRequest { get; set; } = default!;
}