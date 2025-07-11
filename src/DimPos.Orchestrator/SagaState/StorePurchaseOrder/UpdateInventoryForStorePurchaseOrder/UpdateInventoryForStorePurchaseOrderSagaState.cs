using MassTransit;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder;

public class UpdateInventoryForStorePurchaseOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}