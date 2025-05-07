using MassTransit;
using SharedProject.Events.Account;

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
            x.CorrelateById(ctx => ctx.CorrelationId ?? Guid.NewGuid());
            x.SelectId(ctx => ctx.CorrelationId ?? Guid.NewGuid());
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(CreateBrandAccount)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateBrandAccount for {context.CorrelationId!.Value}");
                })
                .Produce(context => context.Init<CreateBrandAccountModel>(
                    new CreateBrandAccountModel
                    {
                        BrandId = context.Message.BrandId,
                        Code = context.Message.Code,
                        Email = context.Message.Email,
                        Password = context.Message.Password,
                        Username = context.Message.Username,
                    }))
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
                // .Produce()
                // .TransitionTo()
            
        );
        SetCompletedWhenFinalized();
    }
    
    public Event<CreateBrandAccountModel> CreateBrandAccount { get; private set; }
    public Event<CreateBrandAccountResponseModel> CreateBrandAccountResponse { get; private set; }
    public Event<CreateBrandAccountErrorModel> CreateBrandAccountError { get; private set; }
    public State CreateBrandAccountState { get; private set; }
    public State CreateBrandAccountFailed { get; private set; }
    public State CreateBrandAccountSuccess { get; private set; }
}