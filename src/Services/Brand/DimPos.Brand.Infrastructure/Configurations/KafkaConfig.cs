using Confluent.Kafka;
using DimPos.Brand.Infrastructure.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedProject.Events.Brand;

namespace DimPos.Brand.Infrastructure.Configurations;

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
                    // k.Host("pkc-ldvr1.asia-southeast1.gcp.confluent.cloud:9092", h =>
                    // {
                    //     h.UseSasl(s =>
                    //     {
                    //         s.Username = "J6HIFSCNM5LSKQ2C";
                    //         s.Password = "6tsMrahMDIh3y6+lwt1/MqSt4r+dXw00lqkp3jO2thl5ATICHYXLK4+6SqMPAJCY";
                    //         s.Mechanism = SaslMechanism.Plain;
                    //         s.SecurityProtocol = SecurityProtocol.SaslSsl;
                    //     });
                    // });
                });
                configureRider.AddProducer<Null, CreateBrandAccountModel>(kafkaOptions!.Topics.CreateBrandResponse);
            });
        });
        return services;
    }
}