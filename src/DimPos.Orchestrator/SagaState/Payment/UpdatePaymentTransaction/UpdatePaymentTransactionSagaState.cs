using MassTransit;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction;

public class UpdatePaymentTransactionSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}