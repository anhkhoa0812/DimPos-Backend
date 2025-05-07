using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Account;

namespace DimPos.Identity.Application.Consumers;

public class CreateBrandAccountRequestConsumer : IConsumer<CreateBrandAccountModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, CreateBrandAccountResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, CreateBrandAccountErrorModel> _errorTopicProducer;
    public CreateBrandAccountRequestConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, CreateBrandAccountResponseModel> successTopicProducer,
        ITopicProducer<Null, CreateBrandAccountErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    public async Task Consume(ConsumeContext<CreateBrandAccountModel> context)
    {
        var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(
            predicate: x => x.Name!.Equals("Brand")
        );
        var account = new Accounts()
        {
            Id = Guid.CreateVersion7(),
            Code = context.Message.Code,
            Email = context.Message.Email,
            Username = context.Message.Username,
            RoleId = role.Id,
            Status = EAccountStatus.Active,
        };
        await _unitOfWork.GetRepository<Accounts>().InsertAsync(account);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            await _successTopicProducer.Produce(
                key: null,
                value: new CreateBrandAccountResponseModel()
                {
                    AccountId = account.Id,
                    BrandId = context.Message.BrandId
                },
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
        else
        {
            await _errorTopicProducer.Produce(
                key: null,
                value: new CreateBrandAccountErrorModel()
                {
                    BrandId = context.Message.BrandId
                },
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}