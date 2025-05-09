using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Account;

namespace DimPos.Orchestrator.SagaState.Brands.CreateBrandSaga.Activities;

public class CreateBrandAccountActivity : IStateMachineActivity<CreateBrandSagaState, CreateBrandAccountModel>
{
    private readonly ITopicProducer<Null, CreateBrandAccountModel> _producer;
    public CreateBrandAccountActivity(ITopicProducer<Null, CreateBrandAccountModel> producer)
    {
        _producer = producer;
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateBrandAccountActivity");
    }

    public async Task Execute(BehaviorContext<CreateBrandSagaState, CreateBrandAccountModel> context, IBehavior<CreateBrandSagaState, CreateBrandAccountModel> next)
    {
        var createBrandAccountModel = new CreateBrandAccountModel
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            AccountId = context.Message.AccountId,
            Code = context.Message.Code,
            Email = context.Message.Email,
            HashPassword = context.Message.HashPassword,
            SaltPassword = context.Message.SaltPassword,
            Username = context.Message.Username,
        };
        await _producer.Produce(
            key: null,
            createBrandAccountModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
        
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateBrandSagaState, CreateBrandAccountModel, TException> context, IBehavior<CreateBrandSagaState, CreateBrandAccountModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}