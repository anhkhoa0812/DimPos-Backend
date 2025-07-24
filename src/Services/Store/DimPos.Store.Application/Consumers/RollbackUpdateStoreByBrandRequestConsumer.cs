using DimPos.Store.Domain.Enums;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Store.Application.Consumers;

public class RollbackUpdateStoreByBrandRequestConsumer : IConsumer<RollbackUpdateStoreByBrandRequestModel>
{
    private readonly IUnitOfWork<StoreContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackUpdateStoreByBrandRequestConsumer(IUnitOfWork<StoreContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackUpdateStoreByBrandRequestModel> context)
    {
        var store = await _unitOfWork.GetRepository<Domain.Entities.Store>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.StoreId
        );
        
        if (context.Message.Status == StoreStatus.Active)
        {
            store.Status = EStoreStatus.Inactive;
        }
        else if (context.Message.Status == StoreStatus.Inactive)
        {
            store.Status = EStoreStatus.Active;
        }
        else
        {
            _logger.Warning("Unknown status: {Status} for store with ID: {StoreId}", context.Message.Status, context.Message.StoreId);
        }
        _unitOfWork.GetRepository<Domain.Entities.Store>().UpdateAsync(store);
        await _unitOfWork.CommitAsync();
    }
}