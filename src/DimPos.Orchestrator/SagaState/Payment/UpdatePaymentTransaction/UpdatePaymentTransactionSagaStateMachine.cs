using DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction;

public class UpdatePaymentTransactionSagaStateMachine : MassTransitStateMachine<UpdatePaymentTransactionSagaState>
{
    private readonly ILogger _logger;
    
    public UpdatePaymentTransactionSagaStateMachine(ILogger logger)
    {
        _logger = logger;
        InstanceState(x => x.CurrentState);
        Event(() => CallbackPaymentResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdatePaymentTransactionResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateOrderStatusResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateOrderStatusErrorEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(CallbackPaymentResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Received UpdatePaymentTransactionRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Received CallbackPaymentResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<UpdatePaymentTransactionActivity>())
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Published UpdatePaymentTransactionRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Published UpdatePaymentTransactionRequest for {CorrelationId}", context.CorrelationId!.Value);
                })
                .TransitionTo(UpdatePaymentTransactionState)
        );
        During(UpdatePaymentTransactionState,
            When(UpdatePaymentTransactionResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Received UpdatePaymentTransactionResponse for {context.CorrelationId!.Value}");
                })
                .Activity(config => config.OfType<UpdateOrderStatusActivity>())
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Published UpdateOrderStatusRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Published UpdateOrderStatusRequest for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<RollbackInventoryForFailedPaymentActivity>())
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Published RollbackInventoryForFailedPaymentRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Published RollbackInventoryForFailedPaymentRequest for {CorrelationId}", context.CorrelationId!.Value);
                })
                .TransitionTo(UpdateOrderStatusState)
        );
        During(UpdateOrderStatusState,
            When(UpdateOrderStatusResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Received UpdateOrderStatusResponse for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Received UpdateOrderStatusResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize(),
            When(UpdateOrderStatusErrorEvent)
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Received UpdateOrderStatusError for {context.CorrelationId!.Value}");
                    _logger.Error("[Saga] Received UpdateOrderStatusError for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<RollbackPaymentTransactionActivity>())
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Published RollbackPaymentTransactionRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Published RollbackPaymentTransactionRequest for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<RollbackInventoryForOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine(
                        $"[Saga] Published RollbackInventoryForOrderRequest for {context.CorrelationId!.Value}");
                    _logger.Information("[Saga] Published RollbackInventoryForOrderRequest for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<CallbackPaymentResponseModel> CallbackPaymentResponseEvent { get; private set; } = null!;
    public Event<UpdatePaymentTransactionResponseModel> UpdatePaymentTransactionResponseEvent { get; private set; } = null!;
    public Event<UpdateOrderStatusResponseModel> UpdateOrderStatusResponseEvent { get; private set; } = null!;
    public Event<UpdateOrderStatusErrorModel> UpdateOrderStatusErrorEvent { get; private set; } = null!;
    public State UpdatePaymentTransactionState { get; private set; } = null!;
    public State UpdateOrderStatusState { get; private set; } = null!;
}