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
}