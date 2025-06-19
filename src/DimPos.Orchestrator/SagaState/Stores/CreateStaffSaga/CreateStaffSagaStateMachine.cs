using DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga.Activities;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga;

public class CreateStaffSagaStateMachine : MassTransitStateMachine<CreateStaffSagaState>
{
    public CreateStaffSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CreateStaffResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateStaffAccountResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateStaffAccountErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });

        Initially(
            When(CreateStaffResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStaff for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<CreateStaffAccountActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published CreateStaffAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStaffAccountState)
        );
        During(CreateStaffAccountState,
            When(CreateStaffAccountResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStaffAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStaffAccountSuccessState)
                .Finalize(),
            When(CreateStaffAccountErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateStaffAccountError for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<RollbackStaffStoreAccountActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Rolled back staff store account for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateStaffAccountFailedState)
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<CreateStaffResponseModel> CreateStaffResponseEvent { get; private set; } = null!;
    public Event<CreateStaffAccountResponseModel> CreateStaffAccountResponseEvent { get; private set; } = null!;
    public Event<CreateStaffAccountErrorModel> CreateStaffAccountErrorEvent { get; private set; } = null!;
    
    public State CreateStaffAccountState { get; private set; } = null!;
    public State CreateStaffAccountFailedState { get; private set; } = null!;
    public State CreateStaffAccountSuccessState { get; private set; } = null!;
}