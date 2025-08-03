using MassTransit;

namespace DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder;

public class UpdatePaymentTransactionForCashOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
}