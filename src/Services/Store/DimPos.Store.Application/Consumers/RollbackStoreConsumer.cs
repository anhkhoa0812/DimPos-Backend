using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Store.Application.Consumers;

public class RollbackStoreConsumer : IConsumer<RollbackStoreRequestModel>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackStoreConsumer(IUnitOfWork<StoreContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<RollbackStoreRequestModel> context)
    {
        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.StoreId
        );
        if (store is null)
        {
            _logger.Warning("Store with ID {StoreId} not found for rollback.", context.Message.StoreId);
            return;
        }

        _unitOfWork.GetRepository<Domain.Entities.Store>().DeleteAsync(store);
        
        var storeAccount = await _unitOfWork.GetRepository<Domain.Entities.StoreAccounts>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.AccountId && x.StoreId == context.Message.StoreId
        );
        if (storeAccount is null)
        {
            _logger.Warning("Store account with ID {AccountId} not found for rollback.", context.Message.AccountId);
            return;
        }
        _unitOfWork.GetRepository<Domain.Entities.StoreAccounts>().DeleteAsync(storeAccount);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            _logger.Information("Rollback successful for Store ID {StoreId} and Account ID {AccountId}.", context.Message.StoreId, context.Message.AccountId);
        }
        else
        {
            _logger.Error("Rollback failed for Store ID {StoreId} and Account ID {AccountId}.", context.Message.StoreId, context.Message.AccountId);
        }
    }
}