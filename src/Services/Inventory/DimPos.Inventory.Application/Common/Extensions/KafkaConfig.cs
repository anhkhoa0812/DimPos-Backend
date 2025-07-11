using Confluent.Kafka;
using DimPos.Inventory.Application.Consumers;
using DimPos.Inventory.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Inventory.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, UpdateInventoryForInternalOrderResponseModel>(kafkaOptions!.Topics.UpdateInventoryForInternalOrderResponse);
                configureRider.AddProducer<Null, UpdateInventoryForInternalOrderErrorModel>(kafkaOptions!.Topics.UpdateInventoryForInternalOrderError);
                
                configureRider.AddConsumer<UpdateInventoryForInternalOrderRequestConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForInternalOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForInternalOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateInventoryForInternalOrderRequestConsumer>(riderContext);
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