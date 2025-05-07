using Confluent.Kafka;

namespace DimPos.Identity.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string CreateBrandAccountRequest { get; set; } = default!;
    public string CreateBrandAccountResponse { get; set; } = default!;
    public string CreateBrandAccountError { get; set; } = default!;
}