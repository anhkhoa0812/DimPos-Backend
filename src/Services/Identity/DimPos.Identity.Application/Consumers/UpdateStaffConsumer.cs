using Confluent.Kafka;
using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.UpdateStaff;
using EAccountStatus = DimPos.Identity.Domain.Enum.EAccountStatus;

namespace DimPos.Identity.Application.Consumers;

public class UpdateStaffConsumer : IConsumer<UpdateStaffRequestModel>
{
    private readonly IUnitOfWork<IdentityContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public UpdateStaffConsumer(IUnitOfWork<IdentityContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<UpdateStaffRequestModel> context)
    {
        var account = await _unitOfWork.GetRepository<Accounts>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.StaffId && x.Role.Name == ERoleName.Staff
        );

        if (account != null)
        {
            account.Code = context.Message.Code ?? account.Code;
            account.Email = context.Message.Email ?? account.Email;
            account.Username = context.Message.Username ?? account.Username;
            account.Status = (EAccountStatus?) context.Message.Status ?? account.Status;
            if (context.Message.HashPassword != null && context.Message.SaltPassword != null)
            {
                account.PasswordHash = context.Message.HashPassword;
                account.PasswordSalt = context.Message.SaltPassword;
            }
            _unitOfWork.GetRepository<Accounts>().UpdateAsync(account);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (isSuccess)
            {
                _logger.Information("Cập nhật thông tin nhân viên thành công: {StaffId}", context.Message.StaffId);
            }
            else
            {
                _logger.Error("Cập nhật thông tin nhân viên thất bại: {StaffId}", context.Message.StaffId);
            }
        }
    }
}