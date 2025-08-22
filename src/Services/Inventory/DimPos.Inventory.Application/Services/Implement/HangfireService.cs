using Confluent.Kafka;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Hangfire;
using MassTransit;
using SharedProject.Events.Notification;

namespace DimPos.Inventory.Application.Services.Implement;

public class HangfireService : IHangfireService
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly ITopicProducer<Null, SendNotificationForMultipleAccountRequestModel> _topicProducer;
    
    public HangfireService(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger, 
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        ITopicProducer<Null, SendNotificationForMultipleAccountRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async Task CheckReOrderLevelAsync()
    {
        var inventoryStocks = await _unitOfWork.GetRepository<InventoryStock>()
            .GetListAsync(
                predicate: x => x.ReOrderLevel >= x.Quantity
            );
        if (inventoryStocks != null && inventoryStocks.Any())
        {
            var storeIds = inventoryStocks.Select(x => x.StoreId.ToString()).Distinct().ToList();
            var accountIdGrpcResponse = await _storeGrpcService.GetAccountIdsByStoreIdsAsync(
                new GetAccountIdsByStoreIdsRequest()
                {
                    StoreIds = { storeIds }
                }
            );
            var sendNotificationForMultipleAccountRequestModel = new SendNotificationForMultipleAccountRequestModel();
            foreach (var inventoryStock in inventoryStocks)
            {
                var accountIds =
                    accountIdGrpcResponse.Responses
                        .Where(x => x.StoreId == inventoryStock.StoreId.ToString()).ToList();

                if (accountIds.Any())
                {
                    var sendNotificationForAccountRequests = accountIds.Select(x => new SendNotificationForAccountRequest()
                    {
                        AccountId = Guid.Parse(x.AccountId),
                        Message =
                            $"Số lượng hàng hoá {inventoryStock.IngredientId} đã đạt mức tối thiểu, vui lòng dặt hàng.",
                        Type = NotificationType.Information
                    }).ToList();
                    sendNotificationForMultipleAccountRequestModel.Accounts.AddRange(sendNotificationForAccountRequests);
                }
            }
            _logger.Information("HangfireService: CheckReOrderLevelAsync - Found {Count} inventory stocks below re-order level",
                inventoryStocks.Count);
            if (sendNotificationForMultipleAccountRequestModel.Accounts.Any())
            {
                await _topicProducer.Produce(
                    null,
                    sendNotificationForMultipleAccountRequestModel,
                    CancellationToken.None
                );
                _logger.Information(
                    "HangfireService: CheckReOrderLevelAsync - Notifications sent for re-order levels for accounts: {@Accounts}",
                    sendNotificationForMultipleAccountRequestModel.Accounts
                );
            }
        }
    }
}