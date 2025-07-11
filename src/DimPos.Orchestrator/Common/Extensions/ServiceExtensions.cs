using Confluent.Kafka;
using DimPos.Orchestrator.Common.Models.Settings;
using DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;
using DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga;
using DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu;
using DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu;
using DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder;
using DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;
using SharedProject.Events.Brand;
using SharedProject.Events.RemoveMenuForStore;
using SharedProject.Events.Store.CreateStaff;
using SharedProject.Events.Store.CreateStore;
using SharedProject.Events.UpdateInventoryForInternalOrder;

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
                rider
                    .AddSagaStateMachine<AssignNewStoreMenuStateMachine, AssignNewStoreMenuSagaState>().InMemoryRepository();
                rider
                    .AddSagaStateMachine<RemoveStoreMenuStateMachine, RemoveStoreMenuSagaState>().InMemoryRepository();
                rider
                    .AddSagaStateMachine<CreateStaffSagaStateMachine, CreateStaffSagaState>().InMemoryRepository();
                rider
                    .AddSagaStateMachine<UpdateInventoryForStorePurchaseOrderStateMachine,
                        UpdateInventoryForStorePurchaseOrderSagaState>().InMemoryRepository();
                //Add Producers
                rider.AddProducer<Null, CreateBrandAccountModel>(kafkaOptions!.Topics.CreateBrandAccountRequest);
                rider.AddProducer<Null, RollbackBrandAccountModel>(kafkaOptions!.Topics.RollbackBrandAccountRequest);
                rider.AddProducer<Null, CreateStoreAccountRequestModel>(kafkaOptions!.Topics.CreateStoreAccountRequest);
                rider.AddProducer<Null, RollbackStoreRequestModel>(kafkaOptions!.Topics.RollbackStoreAccountRequest);
                rider.AddProducer<Null, AddStorePriceRequestModel>(kafkaOptions!.Topics.AddStorePriceRequest);
                rider.AddProducer<Null, RollbackStoreMenuModel>(kafkaOptions!.Topics.RollbackStoreMenuRequest);
                rider.AddProducer<Null, RemoveStorePriceRequestModel>(kafkaOptions!.Topics.RemoveStorePriceRequest);
                rider.AddProducer<Null, RollbackRemoveStoreMenuModel>(kafkaOptions!.Topics.RollbackRemoveStoreMenuRequest);
                rider.AddProducer<Null, CreateStaffAccountRequestModel>(kafkaOptions!.Topics.CreateStaffAccountRequest);
                rider.AddProducer<Null, RollbackStaffStoreAccountRequestModel>(kafkaOptions!.Topics.RollbackStaffStoreAccountRequest);
                rider.AddProducer<Null, GetIngredientDetailsRequestModel>(kafkaOptions!.Topics.GetIngredientDetailsRequest);
                rider.AddProducer<Null, UpdateInventoryForInternalOrderRequestModel>(kafkaOptions.Topics
                    .UpdateInventoryForInternalOrderRequest);
                rider.AddProducer<Null, ChangeErrorStatusForStorePurchaseOrderRequestModel>(
                    kafkaOptions.Topics.ChangeErrorStatusForStorePurchaseOrderRequest);
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
                    //Assign Store Price
                    kafkaConfig.TopicEndpoint<Null, AssignNewStoreMenuModel>(
                        topicName: kafkaOptions!.Topics.AssignNewStoreMenuResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<AssignNewStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, AddStorePriceResponseModel>(
                        topicName: kafkaOptions!.Topics.AddStorePriceResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<AssignNewStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, AddStorePriceErrorModel>(
                        topicName: kafkaOptions!.Topics.AddStorePriceError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<AssignNewStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    //Remove Store Price
                    kafkaConfig.TopicEndpoint<Null, RemoveStoreMenuModel>(
                        topicName: kafkaOptions!.Topics.RemoveStoreMenuResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<RemoveStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, RemoveStorePriceResponseModel>(
                        topicName: kafkaOptions!.Topics.RemoveStorePriceResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<RemoveStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, RemoveStorePriceErrorModel>(
                        topicName: kafkaOptions!.Topics.RemoveStorePriceError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<RemoveStoreMenuSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    //Create Staff
                    kafkaConfig.TopicEndpoint<Null, CreateStaffResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateStaffResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStaffSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, CreateStaffAccountResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateStaffAccountResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStaffSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    kafkaConfig.TopicEndpoint<Null, CreateStaffAccountErrorModel>(
                        topicName: kafkaOptions!.Topics.CreateStaffAccountError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<CreateStaffSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });  
                    //Update Inventory for Store Purchase Order
                    kafkaConfig.TopicEndpoint<Null, InternalOrderDoneByStoreResponseModel>(
                        topicName: kafkaOptions!.Topics.InternalOrderDoneByStoreResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForStorePurchaseOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, GetIngredientDetailsResponseModel>(
                        topicName: kafkaOptions!.Topics.GetIngredientDetailsResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForStorePurchaseOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForInternalOrderErrorModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForInternalOrderError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForStorePurchaseOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForInternalOrderResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForInternalOrderResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForStorePurchaseOrderSagaState>(riderContext);
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