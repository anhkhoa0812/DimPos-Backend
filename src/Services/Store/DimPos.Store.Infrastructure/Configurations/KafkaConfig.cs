using DimPos.Store.Infrastructure.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            });
        });
        return services;
    }
}