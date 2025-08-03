using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder.Activities;

public class RollbackPendingForCashOrderActivity :  IStateMachineActivity<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel>
{
    private readonly ITopicProducer<Null, RollbackPendingForCashOrderRequestModel> _topicProducer;
    
    public RollbackPendingForCashOrderActivity(
        ITopicProducer<Null, RollbackPendingForCashOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackPendingForCashOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> next)
    {
        var rollbackPendingForCashOrderRequestModel = new RollbackPendingForCashOrderRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = context.Message.OrderId
        };
        await _topicProducer.Produce(
            null,
            rollbackPendingForCashOrderRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel, TException> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}