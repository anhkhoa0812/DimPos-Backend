using MassTransit;

namespace DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand;

public class UpdateStoreByBrandSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}