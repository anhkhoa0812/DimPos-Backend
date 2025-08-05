using MassTransit;

namespace DimPos.Orchestrator.SagaState.Orders.CancelOrder;

public class CancelOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}