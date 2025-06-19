using DimPos.Store.Domain.Entities;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Store.Application.Consumers;

public class RollbackStaffStoreAccountRequestConsumer : IConsumer<RollbackStaffStoreAccountRequestModel>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackStaffStoreAccountRequestConsumer(IUnitOfWork<StoreContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackStaffStoreAccountRequestModel> context)
    {
        var storeAccount = await _unitOfWork.GetRepository<StoreAccounts>().SingleOrDefaultAsync(
            predicate: x => x.StoreId == context.Message.StoreId
                && x.AccountId == context.Message.AccountId
        );
        if (storeAccount == null)
        {
            _logger.Error("Failed to rollback staff store account for StoreId: {StoreId}, AccountId: {AccountId}",
                context.Message.StoreId, context.Message.AccountId);
            return;
        }
        _unitOfWork.GetRepository<StoreAccounts>().DeleteAsync(storeAccount);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            _logger.Information("Rollback staff store account successfully for StoreId: {StoreId}, AccountId: {AccountId}",
                context.Message.StoreId, context.Message.AccountId);
        }
        else
        {
            _logger.Error("Failed to rollback staff store account for StoreId: {StoreId}, AccountId: {AccountId}",
                context.Message.StoreId, context.Message.AccountId);
        }
    }
}