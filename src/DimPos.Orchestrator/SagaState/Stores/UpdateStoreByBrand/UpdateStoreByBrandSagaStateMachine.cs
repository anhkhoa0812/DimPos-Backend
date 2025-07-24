using DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand.Activities;
using MassTransit;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand;

public class UpdateStoreByBrandSagaStateMachine : MassTransitStateMachine<UpdateStoreByBrandSagaState>
{
    public UpdateStoreByBrandSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        Event(() => UpdateStoreByBrandRequestEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateAccountForStoreByBrandResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateAccountForStoreByBrandErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });

        Initially(
            When(UpdateStoreByBrandRequestEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateStoreByBrandRequest for {context.CorrelationId!.Value}");
                })
                .Activity(x => x.OfType<UpdateAccountForStoreByBrandActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Completed UpdateStoreByBrandRequest for {context.CorrelationId!.Value}");
                })
                .TransitionTo(UpdateAccountForStoreByBrandState)
        );
        During(UpdateAccountForStoreByBrandState,
            When(UpdateAccountForStoreByBrandResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateAccountForStoreByBrandResponse for {context.CorrelationId!.Value}");
                })
                .Finalize(),
            When(UpdateAccountForStoreByBrandErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateAccountForStoreByBrandError for {context.CorrelationId!.Value}");
                })
                .Activity(x => x.OfType<RollbackUpdateStoreByBrandActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Rolled back UpdateStoreByBrandRequest for {context.CorrelationId!.Value}");
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    
    public Event<UpdateStoreByBrandRequestModel> UpdateStoreByBrandRequestEvent { get; private set; } = null!;
    public Event<UpdateAccountForStoreByBrandResponseModel> UpdateAccountForStoreByBrandResponseEvent { get; private set; } = null!;
    public Event<UpdateAccountForStoreByBrandErrorModel> UpdateAccountForStoreByBrandErrorEvent { get; private set; } = null!;
    public State UpdateAccountForStoreByBrandState { get; private set; } = null!;
}