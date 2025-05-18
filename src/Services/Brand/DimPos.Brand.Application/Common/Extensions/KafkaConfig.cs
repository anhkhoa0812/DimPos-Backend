using Confluent.Kafka;
using DimPos.Brand.Application.Consumers;
using DimPos.Brand.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Brand;

namespace DimPos.Brand.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, CreateBrandAccountModel>(kafkaOptions!.Topics.CreateBrandResponse);

                configureRider.AddConsumer<RollbackBrandAccountConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, RollbackBrandAccountModel>(
                        topicName: kafkaOptions!.Topics.RollbackBrandAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackBrandAccountConsumer>(riderContext);
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