using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga.Activities;

public class SendNotificationForCreateStoreAccountErrorActivity : IStateMachineActivity<CreateStoreSagaState, CreateStoreAccountErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public SendNotificationForCreateStoreAccountErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForCreateStoreAccountErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<CreateStoreSagaState, CreateStoreAccountErrorModel> context, IBehavior<CreateStoreSagaState, CreateStoreAccountErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.ErrorMessage,
            Type = NotificationType.Error,
            AccountId = context.Message.BrandAccountId
        };
        await _topicProducer.Produce(
            key: null,
            sendNotificationForAccountRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStoreSagaState, CreateStoreAccountErrorModel, TException> context, IBehavior<CreateStoreSagaState, CreateStoreAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}