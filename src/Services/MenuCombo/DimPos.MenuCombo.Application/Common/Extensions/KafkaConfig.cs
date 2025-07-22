using Confluent.Kafka;
using DimPos.MenuCombo.Application.Consumers;
using DimPos.MenuCombo.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;
using SharedProject.Events.RemoveMenuForStore;
using SharedProject.Events.UpdateBrandMenuItem;

namespace DimPos.MenuCombo.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, AssignNewStoreMenuModel>(kafkaOptions!.Topics.AssignNewStoreMenuResponse);
                configureRider.AddProducer<Null, RemoveStoreMenuModel>(kafkaOptions!.Topics.RemoveStoreMenuResponse);
                configureRider.AddProducer<Null, UpdateBrandMenuItemResponseModel>(kafkaOptions!.Topics.UpdateBrandMenuItemResponse);
                
                configureRider.AddConsumer<RollbackStoreMenuConsumer>();
                configureRider.AddConsumer<RollbackRemoveStoreMenuConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, RollbackStoreMenuModel>(
                        topicName: kafkaOptions!.Topics.RollbackStoreMenuRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackStoreMenuConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RollbackRemoveStoreMenuConsumer>(
                        topicName: kafkaOptions!.Topics.RollbackRemoveStoreMenuRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackRemoveStoreMenuConsumer>(riderContext);
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