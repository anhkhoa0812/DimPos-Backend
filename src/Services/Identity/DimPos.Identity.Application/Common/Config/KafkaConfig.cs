using Confluent.Kafka;
using DimPos.Identity.Application.Consumers;
using DimPos.Identity.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Account;

namespace DimPos.Identity.Application.Common.Config;

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
                configureRider.AddProducer<Null, CreateBrandAccountResponseModel>(kafkaOptions!.Topics.CreateBrandAccountResponse);
                configureRider.AddProducer<Null, CreateBrandAccountErrorModel>(kafkaOptions!.Topics.CreateBrandAccountError);
                configureRider.UsingKafka(kafkaOptions.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, CreateBrandAccountModel>(
                        topicName: kafkaOptions.Topics.CreateBrandAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<CreateBrandAccountRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                });
            });
            
        });
        return services;
    }
}