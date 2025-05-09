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
            predicate: x => x.Name == ERoleName.BrandAdmin
        );
        var account = new Accounts()
        {
            Id = context.Message.AccountId,
            Code = context.Message.Code,
            Email = context.Message.Email,
            Username = context.Message.Username,
            RoleId = role.Id,
            PasswordHash = context.Message.HashPassword,
            PasswordSalt = context.Message.SaltPassword,
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
                    CorrelationId = context.Message.CorrelationId,
                    AccountId = context.Message.AccountId,
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
                    CorrelationId = context.Message.CorrelationId,
                    BrandId = context.Message.BrandId,
                    AccountId = context.Message.AccountId
                },
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}