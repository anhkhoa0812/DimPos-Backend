using Confluent.Kafka;
using DimPos.Store.Infrastructure.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Store.Infrastructure.Configurations;

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
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (context, k) =>
                {
                });
                configureRider.AddProducer<Null, CreateStoreResponseModel>(kafkaOptions!.Topics.CreateStoreResponse);
            });
        });
        return services;
    }
}