using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.UpdateBrandMenuItem;

namespace DimPos.Orchestrator.SagaState.BrandMenuItems.UpdateBrandMenuItem.Activities;

public class CreateStorePriceForBrandMenuItemActivity : IStateMachineActivity<UpdateBrandMenuItemSageState, UpdateBrandMenuItemResponseModel>
{
    private readonly ITopicProducer<Null, CreateStorePriceForBrandMenuItemRequestModel> _topicProducer;
    
    public CreateStorePriceForBrandMenuItemActivity(ITopicProducer<Null, CreateStorePriceForBrandMenuItemRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateStorePriceForBrandMenuItemActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateBrandMenuItemSageState, UpdateBrandMenuItemResponseModel> context, IBehavior<UpdateBrandMenuItemSageState, UpdateBrandMenuItemResponseModel> next)
    {
        var createStorePriceForBrandMenuItemRequestModel = new CreateStorePriceForBrandMenuItemRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            StoreIds = context.Message.StoreIds,
            NewProductVariantIds = context.Message.NewProductVariantIds,
            RemovedProductVariantIds = context.Message.RemovedProductVariantIds
        };
        await _topicProducer.Produce(
            key: null,
            createStorePriceForBrandMenuItemRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateBrandMenuItemSageState, UpdateBrandMenuItemResponseModel, TException> context, IBehavior<UpdateBrandMenuItemSageState, UpdateBrandMenuItemResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}