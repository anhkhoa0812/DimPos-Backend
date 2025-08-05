using DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga.Activities;
using MassTransit;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga;

public class CreateStoreSagaStateMachine : MassTransitStateMachine<CreateStoreSagaState>
{
    public CreateStoreSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        Event(() => CreateStoreResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateStoreAccountResponse, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateStoreAccountError, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(CreateStoreResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStoreAccount for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<CreateStoreAccountActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published CreateStoreAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStoreAccountState));
        During(CreateStoreAccountState,
            When(CreateStoreAccountResponse)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStoreAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStoreAccountSuccess)
                .Finalize(),
            When(CreateStoreAccountError)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStoreAccount for {context.CorrelationId!.Value}");
                })
                .Activity(context => context.OfType<RollbackBrandActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received RollbackBrandActivity for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<SendNotificationForCreateStoreAccountErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published SendNotificationForCreateStoreAccountErrorActivity for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStoreFailed)
                .Finalize()
            );
        SetCompletedWhenFinalized();
    }
    
    public Event<CreateStoreResponseModel> CreateStoreResponseEvent { get; private set; }
    public Event<CreateStoreAccountResponseModel> CreateStoreAccountResponse { get; private set; }
    public Event<CreateStoreAccountErrorModel> CreateStoreAccountError { get; private set; }
    public State CreateStoreAccountState { get; private set; }
    public State CreateStoreFailed { get; private set; }
    public State CreateStoreAccountSuccess { get; private set; }
}