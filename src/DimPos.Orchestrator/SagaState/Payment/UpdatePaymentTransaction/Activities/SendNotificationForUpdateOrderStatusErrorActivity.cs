using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class SendNotificationForUpdateOrderStatusErrorActivity : IStateMachineActivity<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdateOrderStatusErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.Message,
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

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}