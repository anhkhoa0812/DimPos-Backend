using Confluent.Kafka;
using DimPos.Orchestrator.Common.Models.Settings;
using DimPos.Orchestrator.SagaState.BrandMenuItems.UpdateBrandMenuItem;
using DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;
using DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga;
using DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder;
using DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder;
using DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction;
using DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu;
using DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu;
using DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder;
using DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga;
using DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;
using SharedProject.Events.Brand;
using SharedProject.Events.Notification;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;
using SharedProject.Events.Payment.UpdatePaymentTransaction;
using SharedProject.Events.RemoveMenuForStore;
using SharedProject.Events.Store.CreateStaff;
using SharedProject.Events.Store.CreateStore;
using SharedProject.Events.Store.UpdateStoreByBrand;
using SharedProject.Events.UpdateBrandMenuItem;
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
                rider
                    .AddSagaStateMachine<UpdateBrandMenuItemSageStateMachine, 
                        UpdateBrandMenuItemSageState>().InMemoryRepository();
                rider
                    .AddSagaStateMachine<UpdateStoreByBrandSagaStateMachine, UpdateStoreByBrandSagaState>()
                    .InMemoryRepository();
                rider
                    .AddSagaStateMachine<UpdatePaymentTransactionSagaStateMachine, UpdatePaymentTransactionSagaState>()
                    .InMemoryRepository();
                rider.AddSagaStateMachine<UpdateInventoryForSuccessOrderSagaStateMachine,
                    UpdateInventoryForSuccessOrderSagaState>()
                    .InMemoryRepository();
                rider.AddSagaStateMachine<UpdatePaymentTransactionForCashOrderSagaStateMachine,
                        UpdatePaymentTransactionForCashOrderSagaState>()
                    .InMemoryRepository();
                
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
                rider.AddProducer<Null, CreateStorePriceForBrandMenuItemRequestModel>(kafkaOptions.Topics
                    .CreateStorePriceForBrandMenuItemRequest);
                rider.AddProducer<Null, UpdateAccountForStoreByBrandRequestModel>(
                    kafkaOptions.Topics.UpdateAccountForStoreByBrandRequest);
                rider.AddProducer<Null, RollbackUpdateStoreByBrandRequestModel>(
                    kafkaOptions.Topics.RollbackUpdateStoreByBrandRequest);
                rider.AddProducer<Null, UpdatePaymentTransactionRequestModel>(kafkaOptions!.Topics.UpdatePaymentTransactionRequest);
                rider.AddProducer<Null, UpdateOrderStatusRequestModel>(kafkaOptions!.Topics.UpdateOrderStatusRequest);
                rider.AddProducer<Null, RollbackPaymentTransactionRequestModel>(kafkaOptions.Topics.RollbackPaymentTransactionRequest);
                rider.AddProducer<Null, UpdateInventoryForSuccessOrderRequestModel>(kafkaOptions.Topics.UpdateInventoryForSuccessOrderRequest);
                rider.AddProducer<Null, UpdateOrderNeedToChangeInventoryRequestModel>(kafkaOptions.Topics.UpdateOrderNeedToChangeInventoryRequest);
                rider.AddProducer<Null, RollbackInventoryForOrderRequestModel>(kafkaOptions.Topics.RollbackInventoryForOrderRequest);
                rider.AddProducer<Null, UpdatePaymentTransactionForCashOrderRequestModel>(kafkaOptions.Topics.UpdatePaymentTransactionForCashOrderRequest);
                rider.AddProducer<Null, RollbackPendingForCashOrderRequestModel>(kafkaOptions.Topics.RollbackPendingForCashOrderRequest);
                rider.AddProducer<Null, SendNotificationForAccountRequestModel>(kafkaOptions.Topics.SendNotificationForAccountRequest);
                
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
                    //Update Brand Menu Item
                    kafkaConfig.TopicEndpoint<Null, UpdateBrandMenuItemResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdateBrandMenuItemResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateBrandMenuItemSageState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    //Update Store By Brand
                    kafkaConfig.TopicEndpoint<Null, UpdateStoreByBrandRequestModel>(
                        topicName: kafkaOptions!.Topics.UpdateStoreByBrandRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateStoreByBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateAccountForStoreByBrandResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdateAccountForStoreByBrandResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateStoreByBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateAccountForStoreByBrandErrorModel>(
                        topicName: kafkaOptions!.Topics.UpdateAccountForStoreByBrandError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateStoreByBrandSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    //Update Payment Transaction
                    kafkaConfig.TopicEndpoint<Null, CallbackPaymentResponseModel>(
                        topicName: kafkaOptions!.Topics.CallbackPaymentResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdatePaymentTransactionResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdatePaymentTransactionResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateOrderStatusResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdateOrderStatusResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateOrderStatusErrorModel>(
                        topicName: kafkaOptions!.Topics.UpdateOrderStatusError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    //Update Inventory for Success Order
                    kafkaConfig.TopicEndpoint<Null, CreateOrderResponseModel>(
                        topicName: kafkaOptions!.Topics.CreateOrderResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForSuccessOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForSuccessOrderResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForSuccessOrderResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForSuccessOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdateInventoryForSuccessOrderErrorModel>(
                        topicName: kafkaOptions!.Topics.UpdateInventoryForSuccessOrderError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdateInventoryForSuccessOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    
                    //Update Payment Transaction for Cash Order
                    kafkaConfig.TopicEndpoint<Null, ConfirmForCashOrderResponseModel>(
                        topicName: kafkaOptions!.Topics.ConfirmForCashOrderResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionForCashOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdatePaymentTransactionForCashOrderResponseModel>(
                        topicName: kafkaOptions!.Topics.UpdatePaymentTransactionForCashOrderResponse,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionForCashOrderSagaState>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                            topicConfig.UseInMemoryOutbox(riderContext);
                            topicConfig.CreateIfMissing();
                        });
                    kafkaConfig.TopicEndpoint<Null, UpdatePaymentTransactionForCashOrderErrorModel>(
                        topicName: kafkaOptions!.Topics.UpdatePaymentTransactionForCashOrderError,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureSaga<UpdatePaymentTransactionForCashOrderSagaState>(riderContext);
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