using Confluent.Kafka;
using DimPos.Order.Application.Consumers;
using DimPos.Order.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Order.Application.Common.Extensions;

public static class KafkaConfig
{
    internal static IServiceCollection AddCustomKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration
            .GetSection("KafkaOptions")
            .Get<KafkaOptions>();
        
        services.AddMassTransit(configureMassTransit =>
        {
            configureMassTransit.UsingInMemory();
            configureMassTransit.AddRider(configureRider =>
            {
                configureRider.AddProducer<Null, InternalOrderDoneByStoreResponseModel>(kafkaOptions!.Topics.InternalOrderDoneByStoreResponse);
                
                configureRider.AddConsumer<ChangeErrorStatusForStorePurchaseOrderRequestConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, ChangeErrorStatusForStorePurchaseOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.ChangeErrorStatusForStorePurchaseOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<ChangeErrorStatusForStorePurchaseOrderRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                });
            });
        });
        return services;
    }
}