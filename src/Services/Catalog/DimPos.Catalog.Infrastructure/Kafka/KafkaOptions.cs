using Confluent.Kafka;

namespace DimPos.Catalog.Infrastructure.Kafka;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
};
public sealed record Topics
{
    public string AddStorePriceRequest { get; set; } = default!;
    public string AddStorePriceResponse { get; set; } = default!;
    public string AddStorePriceError { get; set; } = default!;
    public string RemoveStorePriceRequest { get; set; } = default!;
    public string RemoveStorePriceResponse { get; set; } = default!;
    public string RemoveStorePriceError { get; set; } = default!;
    public string GetIngredientDetailsRequest { get; set; } = default!;
    public string GetIngredientDetailsResponse { get; set; } = default!;
    public string UpdateInventoryForInternalOrderErrorResponse { get; set; } = default!;
    public string CreateStorePriceForBrandMenuItemRequest { get; set; } = default!;
}