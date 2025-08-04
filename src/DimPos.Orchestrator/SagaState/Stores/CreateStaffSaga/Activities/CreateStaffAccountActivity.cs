using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga.Activities;

public class CreateStaffAccountActivity : IStateMachineActivity<CreateStaffSagaState, CreateStaffResponseModel>
{
    private readonly ITopicProducer<Null, CreateStaffAccountRequestModel> _producer;
    
    public CreateStaffAccountActivity(ITopicProducer<Null, CreateStaffAccountRequestModel> producer)
    {
        _producer = producer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("CreateStaffAccountActivity");
    }

    public async Task Execute(BehaviorContext<CreateStaffSagaState, CreateStaffResponseModel> context, IBehavior<CreateStaffSagaState, CreateStaffResponseModel> next)
    {
        var createStaffAccountModel = new CreateStaffAccountRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreAdminAccountId = context.Message.StoreAdminAccountId,
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
            createStaffAccountModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStaffSagaState, CreateStaffResponseModel, TException> context, IBehavior<CreateStaffSagaState, CreateStaffResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

}