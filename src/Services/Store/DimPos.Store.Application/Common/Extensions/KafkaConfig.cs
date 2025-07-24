using Confluent.Kafka;
using DimPos.Store.Application.Consumers;
using DimPos.Store.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;
using SharedProject.Events.Store.CreateStore;
using SharedProject.Events.Store.UpdateStaff;
using SharedProject.Events.Store.UpdateStore;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Store.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, CreateStoreResponseModel>(kafkaOptions!.Topics.CreateStoreResponse);
                configureRider.AddProducer<Null, CreateStaffResponseModel>(kafkaOptions!.Topics.CreateStaffResponse);
                configureRider.AddProducer<Null, UpdateStaffRequestModel>(kafkaOptions!.Topics.UpdateStaffRequest);
                configureRider.AddProducer<Null, UpdateStoreRequestModel>(kafkaOptions!.Topics.UpdateStoreRequest);
                configureRider.AddProducer<Null, UpdateStoreByBrandRequestModel>(kafkaOptions!.Topics.UpdateStoreByBrandRequest);
                
                configureRider.AddConsumer<RollbackStoreConsumer>();
                configureRider.AddConsumer<RollbackStaffStoreAccountRequestConsumer>();
                configureRider.AddConsumer<RollbackUpdateStoreByBrandRequestConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, RollbackStoreConsumer>(
                        topicName: kafkaOptions!.Topics.RollbackStoreAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackStoreConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RollbackStaffStoreAccountRequestModel>(
                        topicName: kafkaOptions!.Topics.RollbackStaffStoreAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackStaffStoreAccountRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RollbackUpdateStoreByBrandRequestModel>(
                        topicName: kafkaOptions!.Topics.RollbackUpdateStoreByBrandRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackUpdateStoreByBrandRequestConsumer>(riderContext);
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