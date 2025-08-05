using DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu.Activities;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu;

public class AssignNewStoreMenuStateMachine  : MassTransitStateMachine<AssignNewStoreMenuSagaState>
{
    
    public AssignNewStoreMenuStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        Event(() => AssignNewStoreMenu, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => AddStorePriceResponse, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => AddStorePriceError, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        
        Initially(
            When(AssignNewStoreMenu)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received AssignNewStoreMenu for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<AssignNewStoreMenuActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published AssignNewStoreMenu for {context.CorrelationId!.Value}");
                })
                .TransitionTo(AddStorePriceState));
        During(AddStorePriceState,
            When(AddStorePriceResponse)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received AddStorePriceResponse for {context.CorrelationId!.Value}");
                })
                .TransitionTo(AddStorePriceSuccessState)
                .Finalize(),
            When(AddStorePriceError)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received AddStorePriceError for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<AddStorePriceErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published RollbackStoreMenuModel for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<SendNotificationForAddStorePriceErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published SendNotificationForAccountRequestModel for {context.CorrelationId!.Value}");
                })
                .TransitionTo(AddStorePriceErrorState)
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    
    public Event<AssignNewStoreMenuModel> AssignNewStoreMenu { get; private set; } = null!;
    public Event<AddStorePriceResponseModel> AddStorePriceResponse { get; private set; } = null!;
    public Event<AddStorePriceErrorModel> AddStorePriceError { get; private set; } = null!;
    
    public State AddStorePriceState { get; private set; } = null!;
    public State AddStorePriceSuccessState { get; private set; } = null!;
    public State AddStorePriceErrorState { get; private set; } = null!;
}