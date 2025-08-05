using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu.Activities;

public class SendNotificationForRemoveStorePriceErrorActivity : IStateMachineActivity<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;
    
    public SendNotificationForRemoveStorePriceErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForRemoveStorePriceErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> context, IBehavior<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.Message,
            Type = NotificationType.Error,
            AccountId = context.Message.BrandAccountId
        };
        await _topicProducer.Produce(
            key: null,
            sendNotificationForAccountRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel, TException> context, IBehavior<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}