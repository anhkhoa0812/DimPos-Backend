using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder.Activities;

public class SendNotificationForUpdateInventoryForInternalOrderErrorActivity : IStateMachineActivity<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;
    
    public SendNotificationForUpdateInventoryForInternalOrderErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdateInventoryForInternalOrderErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> next)
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
    
    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel, TException> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}