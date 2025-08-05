using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdatePaymentTransactionForCashOrder.Activities;

public class SendNotificationForUpdatePaymentTransactionForCashOrderErrorActivity : IStateMachineActivity<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public SendNotificationForUpdatePaymentTransactionForCashOrderErrorActivity(
        ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdatePaymentTransactionForCashOrderErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.ErrorMessage,
            Type = NotificationType.Error,
            AccountId = context.Message.AccountId
        };
        await _topicProducer.Produce(
            key: null,
            sendNotificationForAccountRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel, TException> context, IBehavior<UpdatePaymentTransactionForCashOrderSagaState, UpdatePaymentTransactionForCashOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}