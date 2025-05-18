using MassTransit;

namespace DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu;

public class RemoveStoreMenuSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}