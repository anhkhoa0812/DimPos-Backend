using Confluent.Kafka;
using DimPos.Orchestrator.Common.Models.Settings;
using DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;
using DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga;
using MassTransit;
using SharedProject.Events.Brand;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Orchestrator.Common.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration
            .GetSection(nameof(KafkaOptions))
            .Get<KafkaOptions>();

        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            massTransit.AddRider(rider =>
            {
                //Add Saga State Machines
                rider
                    .AddSagaStateMachine<CreateBrandSagaStateMachine, CreateBrandSagaState>().InMemoryRepository();
                rider
                    .AddSagaStateMachine<CreateStoreSagaStateMachine, CreateStoreSagaState>().InMemoryRepository();
                
                //Add Producers
                rider.AddProducer<Null, CreateBrandAccountModel>(kafkaOptions!.Topics.CreateBrandAccountRequest);
                rider.AddProducer<Null, RollbackBrandAccountModel>(kafkaOptions!.Topics.RollbackBrandAccountRequest);
                rider.AddProducer<Null, CreateStoreAccountRequestModel>(kafkaOptions!.Topics.CreateStoreAccountRequest);
                rider.AddProducer<Null, RollbackStoreRequestModel>(kafkaOptions!.Topics.RollbackStoreAccountRequest);
                
                
                rider.UsingKafka( kafkaOptions.ClientConfig,(riderContext, kafkaConfig) =>
                {
                    //Create Brand account
                    kafkaConfig.TopicEndpoint<Null, CreateBrandAccountModel>(
                        topicName: kafkaOptions!.Topics.CreateBrandResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, CreateBrandAccountResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateBrandAccountResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, CreateBrandAccountErrorModel>(
                        topicName: kafkaOptions!.Topics.CreateBrandAccountError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    //Create Store account
                    kafkaConfig.TopicEndpoint<Null, CreateStoreResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateStoreResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStoreSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, CreateStoreAccountResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateStoreAccountResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStoreSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, CreateStoreAccountErrorModel>(
                        topicName: kafkaOptions!.Topics.CreateStoreAccountError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStoreSagaState>(riderContext);
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