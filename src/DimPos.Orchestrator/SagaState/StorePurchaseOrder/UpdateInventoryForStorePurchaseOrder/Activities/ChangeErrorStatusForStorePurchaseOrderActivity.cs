using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder.Activities;

public class ChangeErrorStatusForStorePurchaseOrderActivity : IStateMachineActivity<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel>
{
    private readonly ITopicProducer<Null, ChangeErrorStatusForStorePurchaseOrderRequestModel> _topicProducer;
    
    public ChangeErrorStatusForStorePurchaseOrderActivity(ITopicProducer<Null, ChangeErrorStatusForStorePurchaseOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("ChangErrorStatusForStorePurchaseOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> next)
    {
        var changeErrorStatusForStorePurchaseOrderRequestModel = new ChangeErrorStatusForStorePurchaseOrderRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
        };
        await _topicProducer.Produce(
            key: null,
            changeErrorStatusForStorePurchaseOrderRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel, TException> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, UpdateInventoryForInternalOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}