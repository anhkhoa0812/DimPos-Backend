using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Identity.Application.Consumers;

public class CreateStaffAccountRequestConsumer : IConsumer<CreateStaffAccountRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, CreateStaffAccountResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, CreateStaffAccountErrorModel> _errorTopicProducer;
    
public CreateStaffAccountRequestConsumer(
        IUnitOfWork<IdentityContext> unitOfWork,
        ILogger logger,
        ITopicProducer<Null, CreateStaffAccountResponseModel> successTopicProducer,
        ITopicProducer<Null, CreateStaffAccountErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _successTopicProducer = successTopicProducer;
        _errorTopicProducer = errorTopicProducer;
    }
    
    public async Task Consume(ConsumeContext<CreateStaffAccountRequestModel> context)
    {
        _logger.Information($"Received CreateStaffAccountRequestModel for StoreId: {context.Message.StoreId}, AccountId: {context.Message.AccountId}");

        try
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(
                predicate: r => r.Name == ERoleName.Staff
            );
            if (role == null)
            {
                _logger.Error($"Role not found for name: {ERoleName.Staff}");
                throw new BadHttpRequestException("Không tìm thấy vai trò nhân viên");
            }
            var account = new Accounts()
            {
                Id = context.Message.AccountId,
                Code = context.Message.Code,
                Email = context.Message.Email,
                RoleId = role.Id,
                Username = context.Message.Username,
                Status = EAccountStatus.Active,
                PasswordHash = context.Message.HashPassword,
                PasswordSalt = context.Message.SaltPassword
            };
            await _unitOfWork.GetRepository<Accounts>().InsertAsync(account);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (isSuccess)
            {
                _logger.Information($"Successfully created staff account for StoreId: {context.Message.StoreId}, AccountId: {context.Message.AccountId}");
                
                await _successTopicProducer.Produce(
                    key: null,
                    value: new CreateStaffAccountResponseModel()
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
                _logger.Error($"Failed to create staff account for StoreId: {context.Message.StoreId}, AccountId: {context.Message.AccountId}");
                throw new Exception("Không thể tạo tài khoản nhân viên");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "CreateStaffAccountRequestConsumer: {CorrelationId} - Error", context.Message.CorrelationId);
            await _errorTopicProducer.Produce(
                key: null,
                value: new CreateStaffAccountErrorModel() {
                    CorrelationId = context.Message.CorrelationId,
                    AccountId = context.Message.AccountId,
                    StoreId = context.Message.StoreId
                },
                cancellationToken: context.CancellationToken
            );
        }
        
    }
}