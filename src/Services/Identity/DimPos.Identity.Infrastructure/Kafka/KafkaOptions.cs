using Confluent.Kafka;
using Microsoft.Data.SqlClient;

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
    public string CreateStoreAccountRequest { get; set; } = default!;
    public string CreateStoreAccountResponse { get; set; } = default!;
    public string CreateStoreAccountError { get; set; } = default!;
    public string CreateStaffAccountRequest { get; set; } = default!;
    public string CreateStaffAccountResponse { get; set; } = default!;
    public string CreateStaffAccountError { get; set; } = default!;
    public string UpdateStaffRequest { get; set; } = default!;
    public string UpdateStoreRequest { get; set; } = default!;
    public string UpdateAccountForStoreByBrandRequest { get; set; } = default!;
    public string UpdateAccountForStoreByBrandResponse { get; set; } = default!;
    public string UpdateAccountForStoreByBrandError { get; set; } = default!;
    public string UpdateBrandPasswordRequest { get; set; } = default!;
}