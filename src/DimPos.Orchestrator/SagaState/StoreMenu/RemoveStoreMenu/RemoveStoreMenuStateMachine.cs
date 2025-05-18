using DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu.Activities;
using MassTransit;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu;

public class RemoveStoreMenuStateMachine : MassTransitStateMachine<RemoveStoreMenuSagaState>
{
    public RemoveStoreMenuStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => RemoveStoreMenu, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => RemoveStorePriceResponse, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => RemoveStorePriceError, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(RemoveStoreMenu)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received RemoveStoreMenu for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<RemoveStoreMenuActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published RemoveStoreMenu for {context.CorrelationId!.Value}");
                })
                .TransitionTo(RemoveStorePriceState));
        During(RemoveStorePriceState,
            When(RemoveStorePriceResponse)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received RemoveStorePriceResponse for {context.CorrelationId!.Value}");
                })
                .TransitionTo(RemoveStorePriceSuccessState)
                .Finalize(),
            When(RemoveStorePriceError)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received RemoveStorePriceError for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<RemoveStorePriceErrorActivity>())
                .TransitionTo(RemoveStorePriceErrorState)
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<RemoveStoreMenuModel> RemoveStoreMenu { get; private set; }
    public Event<RemoveStorePriceResponseModel> RemoveStorePriceResponse { get; private set; }
    public Event<RemoveStorePriceErrorModel> RemoveStorePriceError { get; private set; }
    public State RemoveStorePriceState { get; private set; }
    public State RemoveStorePriceSuccessState { get; private set; }
    public State RemoveStorePriceErrorState { get; private set; }
}