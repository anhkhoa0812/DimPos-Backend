using DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder.Activities;
using MassTransit;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder;

public class UpdateInventoryForSuccessOrderSagaStateMachine :  MassTransitStateMachine<UpdateInventoryForSuccessOrderSagaState>
{
    private readonly ILogger _logger;
    public UpdateInventoryForSuccessOrderSagaStateMachine(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InstanceState(x => x.CurrentState);
        
        Event(() => CreateOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForSuccessOrderResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Event(() => UpdateInventoryForSuccessOrderErrorResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });

        Initially(
            When(CreateOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received CreateOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received CreateOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<UpdateInventoryForSuccessOrderActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published UpdateInventoryForSuccessOrder for {context.CorrelationId!.Value}");
                    _logger.Information("Published UpdateInventoryForSuccessOrder for {CorrelationId}", context.CorrelationId!.Value);
                })
                .TransitionTo(UpdateInventoryForSuccessOrder)
        );
        During(UpdateInventoryForSuccessOrder,
            When(UpdateInventoryForSuccessOrderResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForSuccessOrderResponse for {context.CorrelationId!.Value}");
                    _logger.Information("Received UpdateInventoryForSuccessOrderResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize(),
            When(UpdateInventoryForSuccessOrderErrorResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateInventoryForSuccessOrderErrorResponse for {context.CorrelationId!.Value}");
                    _logger.Error("Received UpdateInventoryForSuccessOrderErrorResponse for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<UpdateOrderNeedToChangeInventoryActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published UpdateOrderNeedToChangeInventory for {context.CorrelationId!.Value}");
                    _logger.Information("Published UpdateOrderNeedToChangeInventory for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Activity(config => config.OfType<SendNotificationForUpdateInventoryForSuccessOrderErrorActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published SendNotificationForUpdateInventoryForSuccessOrderError for {context.CorrelationId!.Value}");
                    _logger.Information("Published SendNotificationForUpdateInventoryForSuccessOrderError for {CorrelationId}", context.CorrelationId!.Value);
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    
    public Event<CreateOrderResponseModel> CreateOrderResponseEvent { get; private set; } = null!;
    public Event<UpdateInventoryForSuccessOrderResponseModel> UpdateInventoryForSuccessOrderResponseEvent { get; private set; } = null!;
    public Event<UpdateInventoryForSuccessOrderErrorModel> UpdateInventoryForSuccessOrderErrorResponseEvent { get; private set; } = null!;
    public State UpdateInventoryForSuccessOrder { get; private set; } = null!;
}