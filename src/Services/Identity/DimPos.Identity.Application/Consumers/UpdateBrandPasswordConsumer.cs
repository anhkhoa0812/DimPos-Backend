using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.UpdateBrandPassword;

namespace DimPos.Identity.Application.Consumers;

public class UpdateBrandPasswordConsumer : IConsumer<UpdateBrandPasswordRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public UpdateBrandPasswordConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<UpdateBrandPasswordRequestModel> context)
    {
        var account = await _unitOfWork.GetRepository<Accounts>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.AccountId
        );
        if (account == null)
        {
            _logger.Error("Account with ID {AccountId} not found.", context.Message.AccountId);
            throw new BadHttpRequestException("Không tìm thấy tài khoản với ID đã cung cấp.");
        }
        
        account.PasswordHash = context.Message.HashPassword;
        account.PasswordSalt = context.Message.SaltPassword;
        
        _unitOfWork.GetRepository<Accounts>().UpdateAsync(account);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to update password for account with ID {AccountId}.", context.Message.AccountId);
            throw new Exception("Cập nhật mật khẩu không thành công.");
        }
        _logger.Information("Password updated successfully for account with ID {AccountId}.", context.Message.AccountId);
    }
}