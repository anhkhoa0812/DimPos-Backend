using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder.Activities;

public class UpdatePaymentTransactionForCashOrderActivity : IStateMachineActivity<UpdatePaymentTransactionForCashOrderSagaState, ConfirmForCashOrderResponseModel>
{
    private readonly ITopicProducer<Null, UpdatePaymentTransactionForCashOrderRequestModel> _topicProducer;
    
    public UpdatePaymentTransactionForCashOrderActivity(
        ITopicProducer<Null, UpdatePaymentTransactionForCashOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdatePaymentTransactionForCashOrder");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdatePaymentTransactionForCashOrderSagaState, ConfirmForCashOrderResponseModel> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, ConfirmForCashOrderResponseModel> next)
    {
        var updatePaymentTransactionForCashOrderRequestModel = new UpdatePaymentTransactionForCashOrderRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = context.Message.OrderId,
            StoreId = context.Message.StoreId,
            PaymentTransactionId = context.Message.PaymentTransactionId
        };
        await _topicProducer.Produce(
            null,
            updatePaymentTransactionForCashOrderRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionForCashOrderSagaState, ConfirmForCashOrderResponseModel, TException> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, ConfirmForCashOrderResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}