using Confluent.Kafka;
using DimPos.Inventory.Application.Consumers;
using DimPos.Inventory.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Order.CancelOrder;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;
using SharedProject.Events.Payment.UpdatePaymentTransaction;
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
                configureRider.AddProducer<Null, UpdateInventoryForSuccessOrderResponseModel>(kafkaOptions.Topics.UpdateInventoryForSuccessOrderResponse);
                configureRider.AddProducer<Null, UpdateInventoryForSuccessOrderErrorModel>(kafkaOptions.Topics.UpdateInventoryForSuccessOrderError);
                configureRider.AddProducer<Null, UpdateInventoryForCancelOrderResponseModel>(kafkaOptions.Topics.UpdateInventoryForCancelOrderResponse);
                configureRider.AddProducer<Null, UpdateInventoryForCancelOrderErrorModel>(kafkaOptions.Topics.UpdateInventoryForCancelOrderError);
                
                configureRider.AddConsumer<UpdateInventoryForInternalOrderRequestConsumer>();
                configureRider.AddConsumer<UpdateInventoryForSuccessOrderConsumer>();
                configureRider.AddConsumer<RollbackInventoryForOrderConsumer>();
                configureRider.AddConsumer<UpdateInventoryForCancelOrderConsumer>();
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
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForSuccessOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForSuccessOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateInventoryForSuccessOrderConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RollbackInventoryForOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.RollbackInventoryForOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackInventoryForOrderConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForCancelOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForCancelOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateInventoryForCancelOrderConsumer>(riderContext);
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