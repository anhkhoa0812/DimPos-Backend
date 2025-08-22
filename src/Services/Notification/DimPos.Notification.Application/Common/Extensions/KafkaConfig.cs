using Confluent.Kafka;
using DimPos.Notification.Application.Consumers;
using DimPos.Notification.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Notification;

namespace DimPos.Notification.Application.Common.Extensions;

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
                configureRider.AddConsumer<SendNotificationForAccountConsumer>();
                configureRider.AddConsumer<SendNotificationForMultipleAccountRequestConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, SendNotificationForAccountRequestModel>(
                        topicName: kafkaOptions!.Topics.SendNotificationForAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<SendNotificationForAccountConsumer>(
                                riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    kafkaConfig.TopicEndpoint<Null, SendNotificationForMultipleAccountRequestModel>(
                        topicName: kafkaOptions!.Topics.SendNotificationForMultipleAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<SendNotificationForMultipleAccountRequestConsumer>(
                                riderContext);
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