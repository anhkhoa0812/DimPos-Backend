using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Identity.Application.Consumers;

public class CreateStoreAccountRequestConsumer : IConsumer<CreateStoreAccountRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, CreateStoreAccountResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, CreateStoreAccountErrorModel> _errorTopicProducer;
    public CreateStoreAccountRequestConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, CreateStoreAccountResponseModel> successTopicProducer,
        ITopicProducer<Null, CreateStoreAccountErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<CreateStoreAccountRequestModel> context)
    {
        try
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(
                predicate: x => x.Name == ERoleName.StoreAdmin
            );
            var existingAccount = await _unitOfWork.GetRepository<Accounts>().SingleOrDefaultAsync(
                predicate: x => x.Code == context.Message.Code || x.Email == context.Message.Email || x.Username == context.Message.Username
            );
            if (existingAccount != null)
            {
                _logger.Error("Tài khoản đã tồn tại với mã: {Code}, email: {Email}, tên đăng nhập: {Username}",
                    context.Message.Code, context.Message.Email, context.Message.Username);
                throw new BadHttpRequestException("Tài khoản đã tồn tại");
            }
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
                    value: new CreateStoreAccountResponseModel()
                    {
                        CorrelationId = context.Message.CorrelationId,
                        AccountId = context.Message.AccountId,
                        StoreId = context.Message.StoreId
                    },
                    cancellationToken: context.CancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                await ProduceErrorAsync(context);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateStoreAccountRequestConsumer: {CorrelationId} - Failed", context.Message.CorrelationId);
            await ProduceErrorAsync(context);
        }
    }
    private async Task ProduceErrorAsync(ConsumeContext<CreateStoreAccountRequestModel> context)
    {
        await _errorTopicProducer.Produce(
            key: null,
            value: new CreateStoreAccountErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.AccountId,
                StoreId = context.Message.StoreId
            },
            cancellationToken: context.CancellationToken
        ).ConfigureAwait(false);
    }
}