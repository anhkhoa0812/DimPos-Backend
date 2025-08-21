using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Brand;

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
        try
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(
                predicate: x => x.Name == ERoleName.BrandAdmin
            );
            if (role == null)
            {
                throw new BadHttpRequestException("Không tìm thấy role BrandAdmin");
            }
            var existingAccount = await _unitOfWork.GetRepository<Accounts>().SingleOrDefaultAsync(
                predicate: x => x.Code == context.Message.Code 
                                || (!string.IsNullOrEmpty(context.Message.Email) && x.Email == context.Message.Email)
                                || x.Username == context.Message.Username
            );
            if (existingAccount != null)
            {
                _logger.Error("Tài khoản đã tồn tại với mã: {Code}, email: {Email}, tên đăng nhập: {Username}",
                    context.Message.Code, context.Message.Email, context.Message.Username);
                throw new BadHttpRequestException($"Tài khoản thương hiệu vừa tạo đã tồn tại với mã: {context.Message.Code}, email: {context.Message.Email} hoặc tên đăng nhập: {context.Message.Username}");
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
                throw new Exception($"Tạo tài khoản thương hiệu không thành công với tên đăng nhập: {context.Message.Username} và mã: {context.Message.Code}");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateBrandAccountRequestConsumer: {CorrelationId} - Error", context.Message.CorrelationId); 
            await _errorTopicProducer.Produce(
                key: null,
                value: new CreateBrandAccountErrorModel {
                    CorrelationId = context.Message.CorrelationId,
                    AccountId = context.Message.AccountId,
                    BrandId = context.Message.BrandId,
                    ErrorMessage = e.Message,
                    SystemAdminAccountId = context.Message.SystemAdminAccountId
                },
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}