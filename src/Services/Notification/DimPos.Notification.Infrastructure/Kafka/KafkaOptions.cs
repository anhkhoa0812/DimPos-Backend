using Confluent.Kafka;

namespace DimPos.Notification.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string SendNotificationForAccountRequest { get; set; } = default!;
    public string SendNotificationForAccountsRequest { get; set; } = default!;
}