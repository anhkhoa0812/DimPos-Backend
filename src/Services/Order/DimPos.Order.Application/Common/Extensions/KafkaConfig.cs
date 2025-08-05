using Confluent.Kafka;
using DimPos.Order.Application.Consumers;
using DimPos.Order.Infrastructure.Kafka;
using MassTransit;
using SharedProject.Events.Order.CancelOrder;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;
using SharedProject.Events.Payment.UpdatePaymentTransaction;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Order.Application.Common.Extensions;

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
                configureRider.AddProducer<Null, InternalOrderDoneByStoreResponseModel>(kafkaOptions!.Topics.InternalOrderDoneByStoreResponse);
                configureRider.AddProducer<Null, UpdateOrderStatusResponseModel>(kafkaOptions!.Topics.UpdateOrderStatusResponse);
                configureRider.AddProducer<Null, UpdateOrderStatusErrorModel>(kafkaOptions!.Topics.UpdateOrderStatusError);
                configureRider.AddProducer<Null, CreateOrderResponseModel>(kafkaOptions!.Topics.CreateOrderResponse);
                configureRider.AddProducer<Null, ConfirmForCashOrderResponseModel>(kafkaOptions.Topics
                    .ConfirmForCashOrderResponse);
                configureRider.AddProducer<Null, CancelOrderResponseModel>(kafkaOptions.Topics.CancelOrderResponse);
                
                configureRider.AddConsumer<ChangeErrorStatusForStorePurchaseOrderRequestConsumer>();
                configureRider.AddConsumer<UpdateOrderStatusRequestConsumer>();
                configureRider.AddConsumer<UpdateOrderNeedToChangeInventoryConsumer>();
                configureRider.AddConsumer<RollbackPendingForCashOrderConsumer>();
                configureRider.UsingKafka(kafkaOptions!.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<Null, ChangeErrorStatusForStorePurchaseOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.ChangeErrorStatusForStorePurchaseOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<ChangeErrorStatusForStorePurchaseOrderRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateOrderStatusRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateOrderStatusRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateOrderStatusRequestConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateOrderNeedToChangeInventoryRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateOrderNeedToChangeInventoryRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<UpdateOrderNeedToChangeInventoryConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, RollbackPendingForCashOrderRequestModel>(
                        topicName: kafkaOptions!.Topics.RollbackPendingForCashOrderRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<RollbackPendingForCashOrderConsumer>(riderContext);
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