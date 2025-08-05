using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Order.CancelOrder;

namespace DimPos.Orchestrator.SagaState.Orders.CancelOrder.Activities;

public class SendNotificationForUpdateInventoryForCancelOrderErrorActivity : IStateMachineActivity<CancelOrderSagaState, UpdateInventoryForCancelOrderErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;
    
    public SendNotificationForUpdateInventoryForCancelOrderErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdateInventoryForCancelOrderErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<CancelOrderSagaState, UpdateInventoryForCancelOrderErrorModel> context, IBehavior<CancelOrderSagaState, UpdateInventoryForCancelOrderErrorModel> next)
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

    public async Task Faulted<TException>(BehaviorExceptionContext<CancelOrderSagaState, UpdateInventoryForCancelOrderErrorModel, TException> context, IBehavior<CancelOrderSagaState, UpdateInventoryForCancelOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}