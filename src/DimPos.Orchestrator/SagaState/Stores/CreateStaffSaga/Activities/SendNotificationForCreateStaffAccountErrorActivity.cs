using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga.Activities;

public class SendNotificationForCreateStaffAccountErrorActivity : IStateMachineActivity<CreateStaffSagaState, CreateStaffAccountErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;
    
    public SendNotificationForCreateStaffAccountErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateStaffAccountErrorModel");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<CreateStaffSagaState, CreateStaffAccountErrorModel> context, IBehavior<CreateStaffSagaState, CreateStaffAccountErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.Message,
            Type = NotificationType.Error,
            AccountId = context.Message.StoreAdminAccountId
        };
        await _topicProducer.Produce(
            key: null,
            sendNotificationForAccountRequestModel,
            context.CancellationToken);
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStaffSagaState, CreateStaffAccountErrorModel, TException> context, IBehavior<CreateStaffSagaState, CreateStaffAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}