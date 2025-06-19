using MassTransit;

namespace DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga;

public class CreateStaffSagaState: SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}