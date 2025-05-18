using Confluent.Kafka;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.MenuCombo.Application.Consumers;

public class RollbackStoreMenuConsumer : IConsumer<RollbackStoreMenuModel>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackStoreMenuConsumer(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackStoreMenuModel> context)
    {
        _logger.Information("RollbackStoreMenuConsumer: {CorrelationId}", context.Message.CorrelationId);
        var storeMenus = await _unitOfWork.GetRepository<StoreMenuAssignments>().GetListAsync(
            predicate: x => x.BrandMenuId == context.Message.BrandMenuId 
            && context.Message.StoreIds.Contains(x.StoreId)
        );
        if (storeMenus != null && storeMenus.Any())
        {
            _unitOfWork.GetRepository<StoreMenuAssignments>().DeleteRangeAsync(storeMenus);
        }
    }
}