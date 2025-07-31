using MassTransit;

namespace DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder;

public class UpdateInventoryForSuccessOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}