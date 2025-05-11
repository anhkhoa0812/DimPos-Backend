using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Brand;

namespace DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga.Activities;

public class CreateBrandAccountErrorActivity : IStateMachineActivity<CreateBrandSagaState, CreateBrandAccountErrorModel>
{
    private readonly ITopicProducer<Null, RollbackBrandAccountModel> _producer;
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateBrandAccountErrorActivity");
    }


    public async Task Execute(BehaviorContext<CreateBrandSagaState, CreateBrandAccountErrorModel> context,
        IBehavior<CreateBrandSagaState, CreateBrandAccountErrorModel> next)
    {
        var rollbackBrandAccountModel = new RollbackBrandAccountModel
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            AccountId = context.Message.AccountId,
        };
        await _producer.Produce(
            key: null,
            rollbackBrandAccountModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateBrandSagaState, CreateBrandAccountErrorModel, TException> context, IBehavior<CreateBrandSagaState, CreateBrandAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}