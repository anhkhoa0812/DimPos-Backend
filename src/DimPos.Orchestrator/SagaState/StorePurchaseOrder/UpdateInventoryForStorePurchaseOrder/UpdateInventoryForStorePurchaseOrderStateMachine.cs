using Confluent.Kafka;
using DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder.Activities;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder;

public class UpdateInventoryForStorePurchaseOrderStateMachine : MassTransitStateMachine<UpdateInventoryForStorePurchaseOrderSagaState>
{
    public UpdateInventoryForStorePurchaseOrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => InternalOrderDoneByStoreEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => GetIngredientDetailsEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForInternalOrderErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForInternalOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(InternalOrderDoneByStoreEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received InternalOrderDoneByStore for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<GetIngredientDetailsActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published GetIngredientDetailsRequest for {context.CorrelationId!.Value}");
                })
                .TransitionTo(GetIngredientDetailsState)
        );
        During(GetIngredientDetailsState,
            When(GetIngredientDetailsEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received GetIngredientDetailsResponse for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<UpdateInventoryForStorePurchaseOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published UpdateInventoryForInternalOrderRequest for {context.CorrelationId!.Value}");
                })
                .TransitionTo(UpdateInventoryForStorePurchaseOrderState),
            When(UpdateInventoryForInternalOrderErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForInternalOrderError for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<ChangeErrorStatusForStorePurchaseOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published ChangeErrorStatusForStorePurchaseOrder for {context.CorrelationId!.Value}");
                })
                .Finalize()
        );
        During(UpdateInventoryForStorePurchaseOrderState,
            When(UpdateInventoryForInternalOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForInternalOrderResponse for {context.CorrelationId!.Value}");
                })
                .Finalize(),
            When(UpdateInventoryForInternalOrderErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForInternalOrderError for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<ChangeErrorStatusForStorePurchaseOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published ChangeErrorStatusForStorePurchaseOrder for {context.CorrelationId!.Value}");
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<InternalOrderDoneByStoreResponseModel> InternalOrderDoneByStoreEvent { get; private set; } = null!;
    public Event<GetIngredientDetailsResponseModel> GetIngredientDetailsEvent { get; private set; } = null!;
    public Event<UpdateInventoryForInternalOrderErrorModel> UpdateInventoryForInternalOrderErrorEvent { get; private set; } = null!;
    public Event<UpdateInventoryForInternalOrderResponseModel> UpdateInventoryForInternalOrderResponseEvent { get; private set; } = null!;
    public State GetIngredientDetailsState { get; private set; } = null!;
    public State UpdateInventoryForStorePurchaseOrderState { get; private set; } = null!;
    
}