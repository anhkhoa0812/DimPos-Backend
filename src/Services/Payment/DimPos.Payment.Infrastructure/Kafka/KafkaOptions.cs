using Confluent.Kafka;

namespace DimPos.Payment.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};

public sealed record Topics
{
    public string CallbackPaymentResponse { get; set; } = default!;
    public string UpdatePaymentTransactionRequest { get; set; } = default!;
    public string UpdatePaymentTransactionResponse { get; set; } = default!;
    public string RollbackPaymentTransactionRequest { get; set; } = default!;
}