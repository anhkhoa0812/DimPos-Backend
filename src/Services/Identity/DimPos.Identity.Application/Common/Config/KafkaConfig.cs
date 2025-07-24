using Confluent.Kafka;
using DimPos.Identity.Application.Consumers;
using DimPos.Identity.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Brand;
using SharedProject.Events.Store.CreateStaff;
using SharedProject.Events.Store.CreateStore;
using SharedProject.Events.Store.UpdateStaff;
using SharedProject.Events.Store.UpdateStore;
using SharedProject.Events.Store.UpdateStoreByBrand;

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
                configureRider.AddProducer<Null, CreateStoreAccountResponseModel>(kafkaOptions!.Topics.CreateStoreAccountResponse);
                configureRider.AddProducer<Null, CreateStoreAccountErrorModel>(kafkaOptions!.Topics.CreateStoreAccountError);
                configureRider.AddProducer<Null, CreateStaffAccountResponseModel>(kafkaOptions!.Topics.CreateStaffAccountResponse);
                configureRider.AddProducer<Null, CreateStaffAccountErrorModel>(kafkaOptions!.Topics.CreateStaffAccountError);
                configureRider.AddProducer<Null, UpdateAccountForStoreByBrandResponseModel>
                    (kafkaOptions!.Topics.UpdateAccountForStoreByBrandResponse);
                configureRider.AddProducer<Null, UpdateAccountForStoreByBrandErrorModel>
                    (kafkaOptions!.Topics.UpdateAccountForStoreByBrandError);
                
                configureRider.AddConsumer<CreateBrandAccountRequestConsumer>();
                configureRider.AddConsumer<CreateStoreAccountRequestConsumer>();
                configureRider.AddConsumer<CreateStaffAccountRequestConsumer>();
                configureRider.AddConsumer<UpdateStaffConsumer>();
                configureRider.AddConsumer<UpdateStoreRequestConsumer>();
                configureRider.AddConsumer<UpdateAccountForStoreByBrandRequestConsumer>();
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
                    kafkaConfig.TopicEndpoint<Null, CreateStoreAccountRequestModel>(
                        topicName: kafkaOptions.Topics.CreateStoreAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<CreateStoreAccountRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, CreateStaffAccountRequestModel>(
                        topicName: kafkaOptions.Topics.CreateStaffAccountRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<CreateStaffAccountRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateStaffRequestModel>(
                        topicName: kafkaOptions.Topics.UpdateStaffRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateStaffConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateStoreRequestModel>(
                        topicName: kafkaOptions.Topics.UpdateStoreRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateStoreRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateAccountForStoreByBrandRequestModel>(
                        topicName: kafkaOptions.Topics.UpdateAccountForStoreByBrandRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateAccountForStoreByBrandRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.CreateIfMissing();
                        });
                });
            });
            
        });
        return services;
    }
}