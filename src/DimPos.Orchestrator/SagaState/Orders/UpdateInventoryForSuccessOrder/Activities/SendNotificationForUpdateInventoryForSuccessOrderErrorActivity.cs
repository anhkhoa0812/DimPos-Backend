using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Notification;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder.Activities;

public class SendNotificationForUpdateInventoryForSuccessOrderErrorActivity : IStateMachineActivity<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel>
{
    private readonly ITopicProducer<Null, SendNotificationForAccountRequestModel> _topicProducer;

    public SendNotificationForUpdateInventoryForSuccessOrderErrorActivity(ITopicProducer<Null, SendNotificationForAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("SendNotificationForUpdateInventoryForSuccessOrderErrorActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> next)
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

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel, TException> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}