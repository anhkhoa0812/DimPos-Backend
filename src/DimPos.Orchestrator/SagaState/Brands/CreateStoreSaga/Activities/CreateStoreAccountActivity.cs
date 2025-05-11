using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga.Activities;

public class CreateStoreAccountActivity : IStateMachineActivity<CreateStoreSagaState, CreateStoreResponseModel>
{
    private readonly ITopicProducer<Null, CreateStoreAccountRequestModel> _producer;
    public CreateStoreAccountActivity(ITopicProducer<Null, CreateStoreAccountRequestModel> producer)
    {
        _producer = producer;
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateStoreAccountActivity");
    }
    
    public async Task Execute(BehaviorContext<CreateStoreSagaState, CreateStoreResponseModel> context, IBehavior<CreateStoreSagaState, CreateStoreResponseModel> next)
    {
        var createStoreAccountModel = new CreateStoreAccountRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            AccountId = context.Message.AccountId,
            Code = context.Message.Code,
            Email = context.Message.Email,
            HashPassword = context.Message.HashPassword,
            SaltPassword = context.Message.SaltPassword,
            Username = context.Message.Username
        };
        await _producer.Produce(
            key: null,
            createStoreAccountModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
    }
    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStoreSagaState, CreateStoreResponseModel, TException> context, IBehavior<CreateStoreSagaState, CreateStoreResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}