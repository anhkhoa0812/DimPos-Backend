using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;
using SharedProject.Events.Notification;

namespace DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu.Activities;

public class SendNotificationForAddStorePriceErrorActivity : IStateMachineActivity<AssignNewStoreMenuSagaState, AddStorePriceErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public SendNotificationForAddStorePriceErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForAddStorePriceErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> context, IBehavior<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> next)
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

    public async Task Faulted<TException>(BehaviorExceptionContext<AssignNewStoreMenuSagaState, AddStorePriceErrorModel, TException> context, IBehavior<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}