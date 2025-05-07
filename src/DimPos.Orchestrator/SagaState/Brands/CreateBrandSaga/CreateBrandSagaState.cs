using MassTransit;

namespace DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga;

public class CreateBrandSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}