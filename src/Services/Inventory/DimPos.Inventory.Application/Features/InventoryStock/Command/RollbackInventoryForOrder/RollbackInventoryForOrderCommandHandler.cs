using Confluent.Kafka;
using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Order.Application.Common.Protos;
using MassTransit;
using Mediator;
using SharedProject.Events.Order.ChangeIsNeedToUpdateInventoryForOrder;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.RollbackInventoryForOrder;

public class RollbackInventoryForOrderCommandHandler : IRequestHandler<RollbackInventoryForOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly OrderGrpcService.OrderGrpcServiceClient _orderGrpcService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly ITopicProducer<Null, ChangeIsNeedToUpdateInventoryForOrderRequestModel> _topicProducer;
    public RollbackInventoryForOrderCommandHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService, OrderGrpcService.OrderGrpcServiceClient orderGrpcService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        ITopicProducer<Null, ChangeIsNeedToUpdateInventoryForOrderRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _orderGrpcService = orderGrpcService ?? throw new ArgumentNullException(nameof(orderGrpcService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(RollbackInventoryForOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var orderGrpcResponse = await _orderGrpcService.GetIsNeedToUpdateInventoryByOrderIdAsync(
            new GetIsNeedToUpdateInventoryByOrderIdRequest
            {
                OrderId = request.OrderId.ToString(),
                StoreId = storeId.ToString()
            }, cancellationToken: cancellationToken);
        if (!orderGrpcResponse.IsNeedToUpdateInventory)
        {
            throw new BadHttpRequestException("Đơn hàng không cần cập nhật kho hàng");
        }
        var existingTransaction = await _unitOfWork.GetRepository<InventoryTransactions>()
            .SingleOrDefaultAsync(predicate: x => x.RelatedOrderId == request.OrderId && 
                                                  x.Type == EInventoryTransactionType.ConsumptionSale);
        if (existingTransaction != null)
        {
            _logger.Error("Inventory transaction for order {OrderId} already exists", request.OrderId);
            throw new BadHttpRequestException($"Đơn hàng {request.OrderId} đã được cập nhật kho hàng trước đó");
        }
        var recipeItemsResponse = await _catalogGrpcService.GetRecipeItemsByOrderItemsAsync(
            new GetRecipeItemsByOrderItemsRequest()
            {
                OrderItems =
                {
                    orderGrpcResponse.OrderItems.Select(x => new OrderItemForGetRecipeItemsByOrderItemsRequest()
                    {
                        ProductVariantId = x.ProductVariantId,
                        Quantity = x.Quantity
                    }).ToList()
                }
            }
        );
       
        var ingredientGroups = recipeItemsResponse.RecipeItems
            .GroupBy(
                ri => Guid.Parse(ri.Ingredient.Id),
                ri => (decimal) ri.Quantity,
                (ingredientId, quantities) => new
                {
                    IngredientId = ingredientId,
                    TotalQuantity = quantities.Sum()
                })
            .ToList();
        
        var requestIngredientIds = ingredientGroups.Select(x => x.IngredientId).ToList();
        var inventoryStocks = await _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().GetListAsync(
            predicate: x => x.StoreId == storeId && 
                            requestIngredientIds.Contains(x.IngredientId) 
        );
        
        if(inventoryStocks.Count != requestIngredientIds.Count)
        {
            throw new BadHttpRequestException("Lỗi cập nhật kho hàng: Không đủ nguyên liệu trong kho.");
        }

        foreach (var inventoryStock in inventoryStocks)
        {
            var requestIngredient = ingredientGroups.FirstOrDefault(x => x.IngredientId == inventoryStock.IngredientId);

            if (requestIngredient != null)
            {
                if(inventoryStock.Quantity < requestIngredient.TotalQuantity)
                {
                    _logger.Error("Not enough stock for ingredient {IngredientId} in order {OrderId} at store {StoreId} and quantity {Quantity}",
                        requestIngredient.IngredientId, request.OrderId, storeId, requestIngredient.TotalQuantity);
                    throw new BadHttpRequestException($"Lỗi cập nhật kho hàng: Nguyên liệu {requestIngredient.IngredientId} không đủ trong kho");
                }
                
                inventoryStock.Quantity -= requestIngredient.TotalQuantity;
                var newInventoryTransaction = new InventoryTransactions()
                {
                    Id = Guid.CreateVersion7(),
                    Type = EInventoryTransactionType.ConsumptionSale,
                    QuantityChange = -requestIngredient.TotalQuantity,
                    RelatedOrderId = request.OrderId,
                    InventoryStockId = inventoryStock.Id,
                    Note = $"Hoàn tác hoàn trả kho hàng cho đơn hàng {request.OrderId}"
                };
                await _unitOfWork.GetRepository<InventoryTransactions>().InsertAsync(newInventoryTransaction);
                _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().UpdateAsync(inventoryStock);
            }
        }

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to rollback inventory for order {OrderId} at store {StoreId}", request.OrderId, storeId);
            throw new Exception($"Cập nhật kho hàng cho đơn hàng {request.OrderId} không thành công");
        }

        await _topicProducer.Produce(
            null,
            new ChangeIsNeedToUpdateInventoryForOrderRequestModel() 
            {
                CorrelationId = Guid.CreateVersion7(),
                OrderId = request.OrderId
            },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật kho hàng cho đơn hàng thành công",
            Data = request.OrderId
        };
    }
}