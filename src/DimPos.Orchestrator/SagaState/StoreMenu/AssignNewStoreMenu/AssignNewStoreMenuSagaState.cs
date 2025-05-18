using MassTransit;

namespace DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu;

public class AssignNewStoreMenuSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}