using DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga.Activities;
using MassTransit;
using SharedProject.Events.Brand;

namespace DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;

public class CreateBrandSagaStateMachine : MassTransitStateMachine<CreateBrandSagaState>
{
    private readonly IServiceProvider _serviceProvider;

    public CreateBrandSagaStateMachine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InstanceState(x => x.CurrentState);
        
        Event(() => CreateBrandAccount, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateBrandAccountResponse, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => CreateBrandAccountError, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(CreateBrandAccount)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateBrandAccount for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<CreateBrandAccountActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published CreateBrandAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateBrandAccountState));
        During(CreateBrandAccountState,
            When(CreateBrandAccountResponse)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateBrandAccount for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateBrandAccountSuccess)
                .Finalize(),
            When(CreateBrandAccountError)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateBrandAccount for {context.CorrelationId!.Value}");

                })
                .Activity(config => config.OfType<CreateBrandAccountErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published CreateBrandAccount for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<SendNotificationForCreateBrandAccountErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Sent notification for CreateBrandAccount error for {context.CorrelationId!.Value}");
                })
                .TransitionTo(CreateBrandFailed)
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    
    public Event<CreateBrandAccountModel> CreateBrandAccount { get; private set; }
    public Event<CreateBrandAccountResponseModel> CreateBrandAccountResponse { get; private set; }
    public Event<CreateBrandAccountErrorModel> CreateBrandAccountError { get; private set; }
    public State CreateBrandAccountState { get; private set; }
    public State CreateBrandFailed { get; private set; }
    public State CreateBrandAccountSuccess { get; private set; }
    
}