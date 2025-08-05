using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Brand;
using SharedProject.Events.Notification;

namespace DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga.Activities;

public class SendNotificationForCreateBrandAccountErrorActivity : IStateMachineActivity<CreateBrandSagaState, CreateBrandAccountErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public SendNotificationForCreateBrandAccountErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForCreateBrandAccountErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<CreateBrandSagaState, CreateBrandAccountErrorModel> context, IBehavior<CreateBrandSagaState, CreateBrandAccountErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.ErrorMessage,
            Type = NotificationType.Error,
            AccountId = context.Message.SystemAdminAccountId
        };
        await _topicProducer.Produce(
            key: null,
            sendNotificationForAccountRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateBrandSagaState, CreateBrandAccountErrorModel, TException> context, IBehavior<CreateBrandSagaState, CreateBrandAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}