using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand.Activities;

public class SendNotificationForUpdateAccountForStoreByBrandErrorActivity : IStateMachineActivity<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;
    
    public SendNotificationForUpdateAccountForStoreByBrandErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdateAccountForStoreByBrandErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> context, IBehavior<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> next)
    {
        var sendNotificationForAccountRequestModel = new SendNotificationForAccountRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            Message = context.Message.Message,
            Type = NotificationType.Error,
            AccountId = context.Message.BrandAccountId
        };
        await _topicProducer.Produce(
            null,
            sendNotificationForAccountRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel, TException> context, IBehavior<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}