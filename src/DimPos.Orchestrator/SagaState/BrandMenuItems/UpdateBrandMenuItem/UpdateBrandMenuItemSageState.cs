using MassTransit;

namespace DimPos.Orchestrator.SagaState.BrandMenuItems.UpdateBrandMenuItem;

public class UpdateBrandMenuItemSageState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}