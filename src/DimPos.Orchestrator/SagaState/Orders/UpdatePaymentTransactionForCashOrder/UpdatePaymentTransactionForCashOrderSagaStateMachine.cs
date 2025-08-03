using DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder.Activities;
using MassTransit;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder;

public class UpdatePaymentTransactionForCashOrderSagaStateMachine : MassTransitStateMachine<UpdatePaymentTransactionForCashOrderSagaState>
{
    private readonly ILogger _logger;
    public UpdatePaymentTransactionForCashOrderSagaStateMachine(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InstanceState(x => x.CurrentState);
        
        Event(() => ConfirmForCashOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdatePaymentTransactionForCashOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdatePaymentTransactionForCashOrderErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(ConfirmForCashOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received ConfirmForCashOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received ConfirmForCashOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<UpdatePaymentTransactionForCashOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Executed UpdatePaymentTransactionForCashOrderActivity for {context.CorrelationId!.Value}");
                    _logger.Information("Executed UpdatePaymentTransactionForCashOrderActivity for {CorrelationId}", context.CorrelationId!.Value);
                })
                .TransitionTo(UpdatePaymentTransactionForCashOrderState)
        );
        During(UpdatePaymentTransactionForCashOrderState,
            When(UpdatePaymentTransactionForCashOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdatePaymentTransactionForCashOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received UpdatePaymentTransactionForCashOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize(),
            When(UpdatePaymentTransactionForCashOrderErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdatePaymentTransactionForCashOrderError for {context.CorrelationId!.Value}");
                    _logger.Information("Received UpdatePaymentTransactionForCashOrderError for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<RollbackPendingForCashOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Executed RollbackPendingForCashOrderActivity for {context.CorrelationId!.Value}");
                    _logger.Information("Executed RollbackPendingForCashOrderActivity for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<ConfirmForCashOrderResponseModel> ConfirmForCashOrderResponseEvent { get; private set; } = null!;
    public Event<UpdatePaymentTransactionForCashOrderResponseModel> UpdatePaymentTransactionForCashOrderResponseEvent { get; private set; } = null!;
    public Event<UpdatePaymentTransactionForCashOrderErrorModel> UpdatePaymentTransactionForCashOrderErrorEvent { get; private set; } = null!;
    public State UpdatePaymentTransactionForCashOrderState { get; private set; } = null!;
}