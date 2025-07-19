using DimPos.MenuCombo.Application.Common.Mapper;
using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.MenuCombo.Application.Consumers;

public class RollbackRemoveStoreMenuConsumer : IConsumer<RollbackRemoveStoreMenuModel>
{
    private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackRemoveStoreMenuConsumer(IUnitOfWork<MenuComboContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackRemoveStoreMenuModel> context)
    {
        _logger.Information("RollbackRemoveStoreMenuConsumer: {CorrelationId}", context.Message.CorrelationId);
        var storeMenus = context.Message.StoreMenuAssignments.Select(x => new StoreMenuAssignments()
        {
            Id = x.Id,
            StoreId = x.StoreId,
            BrandMenuId = x.BrandMenuId,
            IsActiveAtStore = x.IsActiveAtStore,
            CreatedDate = x.CreatedDate,
            LastModifiedDate = x.LastModifiedDate,
            StoreMenuItemAvailability = x.StoreMenuItemAvailability.Select(x => new StoreMenuItemAvailability()
            {
                Id = x.Id,
                IsActiveAtStore = x.IsActiveAtStore,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate,
                BrandMenuItemId = x.BrandMenuItemId,
            }).ToList()
        }).ToList();

        await _unitOfWork.GetRepository<StoreMenuAssignments>().InsertRangeAsync(storeMenus);

        await _unitOfWork.CommitAsync();
    }
}