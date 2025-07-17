using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.UpdateStore;

namespace DimPos.Identity.Application.Consumers;

public class UpdateStoreRequestConsumer : IConsumer<UpdateStoreRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public UpdateStoreRequestConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<UpdateStoreRequestModel> context)
    {
        var account = await _unitOfWork.GetRepository<Accounts>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.AccountId
        );

        if (account != null)
        {
            account.Username = context.Message.Username ?? account.Username;
            if (context.Message.HashPassword != null && context.Message.SaltPassword != null)
            {
                account.PasswordHash = context.Message.HashPassword;
                account.PasswordSalt = context.Message.SaltPassword;
            }
            
            _unitOfWork.GetRepository<Accounts>().UpdateAsync(account);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (isSuccess)
            {
                _logger.Information("Cập nhật thông tin tài khoản thành công: {AccountId}", context.Message.AccountId);
            }
            else
            {
                _logger.Error("Cập nhật thông tin tài khoản thất bại: {AccountId}", context.Message.AccountId);
            }
        }
        
        else
        {
            _logger.Error("Không tìm thấy tài khoản với ID: {AccountId}", context.Message.AccountId);
            return;
        }
    }
}