using Confluent.Kafka;
using DimPos.Orchestrator.Common.Models.Settings;
using DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;
using MassTransit;
using SharedProject.Events.Account;

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
                rider
                    .AddSagaStateMachine<CreateBrandSagaStateMachine, CreateBrandSagaState>().InMemoryRepository();

                rider.AddProducer<Null, CreateBrandAccountModel>(kafkaOptions!.Topics.CreateBrandAccountRequest);
                rider.AddProducer<Null, RollbackBrandAccountModel>(kafkaOptions!.Topics.RollbackBrandAccountRequest);
                rider.UsingKafka( kafkaOptions.ClientConfig,(riderContext, kafkaConfig) =>
                {
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
                });
            });
        });
        return services;
    }
}