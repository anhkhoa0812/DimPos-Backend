using DimPos.Orchestrator.SagaState.Orders.CancelOrder.Activities;
using MassTransit;
using SharedProject.Events.Order.CancelOrder;

namespace DimPos.Orchestrator.SagaState.Orders.CancelOrder;

public class CancelOrderSagaStateMachine : MassTransitStateMachine<CancelOrderSagaState>
{
    private readonly ILogger _logger;
    public CancelOrderSagaStateMachine(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InstanceState(x => x.CurrentState);
        
        Event(() => CancelOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForCancelOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForCancelOrderErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(CancelOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CancelOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received CancelOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<UpdateInventoryForCancelOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Executing UpdateInventoryForCancelOrderActivity for {context.CorrelationId!.Value}");
                    _logger.Information("Executing UpdateInventoryForCancelOrderActivity for {CorrelationId}", context.CorrelationId!.Value);
                })
                .TransitionTo(UpdateInventoryForCancelOrderState)
        );
        During(UpdateInventoryForCancelOrderState,
            When(UpdateInventoryForCancelOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForCancelOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received UpdateInventoryForCancelOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize(),
            When(UpdateInventoryForCancelOrderErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForCancelOrderError for {context.CorrelationId!.Value}");
                    _logger.Error("Received UpdateInventoryForCancelOrderError for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<SendNotificationForUpdateInventoryForCancelOrderErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Executing SendNotificationForUpdateInventoryForCancelOrderErrorActivity for {context.CorrelationId!.Value}");
                    _logger.Information("Executing SendNotificationForUpdateInventoryForCancelOrderErrorActivity for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize()
            
        );
    }
    public Event<CancelOrderResponseModel> CancelOrderResponseEvent { get; private set; } = null!;
    public Event<UpdateInventoryForCancelOrderResponseModel> UpdateInventoryForCancelOrderResponseEvent { get; private set; } = null!;
    public Event<UpdateInventoryForCancelOrderErrorModel> UpdateInventoryForCancelOrderErrorEvent { get; private set; } = null!;
    
    public State UpdateInventoryForCancelOrderState { get; private set; } = null!;
}