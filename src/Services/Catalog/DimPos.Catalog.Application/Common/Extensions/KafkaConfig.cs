using Confluent.Kafka;
using DimPos.Catalog.Application.Consumers;
using DimPos.Catalog.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Catalog.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, AddStorePriceResponseModel>(kafkaOptions!.Topics.AddStorePriceResponse);
                configureRider.AddProducer<Null, AddStorePriceErrorModel>(kafkaOptions!.Topics.AddStorePriceError);
                configureRider.AddProducer<Null, RemoveStorePriceResponseModel>(kafkaOptions!.Topics.RemoveStorePriceResponse);
                configureRider.AddProducer<Null, RemoveStorePriceErrorModel>(kafkaOptions!.Topics.RemoveStorePriceError);
                
                configureRider.AddConsumer<AddStorePriceRequestConsumer>();
                configureRider.AddConsumer<RemoveStorePriceRequestConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, AddStorePriceRequestModel>(
                        topicName: kafkaOptions!.Topics.AddStorePriceRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<AddStorePriceRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RemoveStorePriceRequestModel>(
                        topicName: kafkaOptions!.Topics.RemoveStorePriceRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RemoveStorePriceRequestConsumer>(riderContext);
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