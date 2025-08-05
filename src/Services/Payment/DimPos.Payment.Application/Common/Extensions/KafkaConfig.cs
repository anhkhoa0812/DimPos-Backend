using Confluent.Kafka;
using DimPos.Payment.Application.Consumers;
using DimPos.Payment.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Order.CancelOrder;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Payment.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, CallbackPaymentResponseModel>(kafkaOptions!.Topics.CallbackPaymentResponse);
                configureRider.AddProducer<Null, UpdatePaymentTransactionResponseModel>(kafkaOptions.Topics.UpdatePaymentTransactionResponse);
                configureRider.AddProducer<Null, UpdatePaymentTransactionForCashOrderResponseModel>(
                    kafkaOptions.Topics.UpdatePaymentTransactionForCashOrderResponse);
                configureRider.AddProducer<Null, UpdatePaymentTransactionForCashOrderErrorModel>(
                    kafkaOptions.Topics.UpdatePaymentTransactionForCashOrderError);
                configureRider.AddProducer<Null, CancelOrderResponseModel>(kafkaOptions.Topics.CancelOrderResponse);
                
                configureRider.AddConsumer<UpdatePaymentTransactionRequestConsumer>();
                configureRider.AddConsumer<RollbackPaymentTransactionConsumer>();
                configureRider.AddConsumer<UpdatePaymentTransactionForCashOrderConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, UpdatePaymentTransactionRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdatePaymentTransactionRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdatePaymentTransactionRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    kafkaConfig.TopicEndpoint<Null, RollbackPaymentTransactionRequestModel>(
                        topicName: kafkaOptions!.Topics.RollbackPaymentTransactionRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackPaymentTransactionConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdatePaymentTransactionForCashOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdatePaymentTransactionForCashOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdatePaymentTransactionForCashOrderConsumer>(riderContext);
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