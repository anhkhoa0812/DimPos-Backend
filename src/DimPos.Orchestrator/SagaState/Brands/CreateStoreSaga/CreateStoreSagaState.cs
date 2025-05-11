using MassTransit;

namespace DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga;

public class CreateStoreSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}