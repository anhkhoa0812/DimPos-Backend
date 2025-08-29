using DimPos.Inventory.Application.Common.Protos;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace DimPos.Inventory.Application.GrpcServices;

public class InventoryGrpcService : Common.Protos.InventoryGrpcService.InventoryGrpcServiceBase
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public InventoryGrpcService(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<CheckInventoryForOrderResponse> CheckInventoryForOrder(CheckInventoryForOrderRequest request, ServerCallContext context)
    {
        _logger.Information("BEGIN: CheckInventoryForOrder");

        var ingredientIds = request.IngredientInventory.Select(x => Guid.Parse(x.IngredientId)).ToList();
        
        var inventoryStocks = await _unitOfWork.GetRepository<InventoryStock>().GetListAsync(
            predicate: x => ingredientIds.Contains(x.IngredientId) && x.StoreId == Guid.Parse(request.StoreId)
        );
        var missingIngredientIds = ingredientIds.Except(inventoryStocks.Select(x => x.IngredientId)).ToList();
        if (missingIngredientIds.Any())
        {
            _logger.Error("Missing inventory stock for ingredients: {MissingIngredientIds}", string.Join(", ", missingIngredientIds));
            return new CheckInventoryForOrderResponse()
            {
                IsValid = false,
                InsufficientIngredientIds = { missingIngredientIds.Select(x => x.ToString()) }
            };
        }

        var response = new CheckInventoryForOrderResponse()
        {
            IsValid = true,
        };
        foreach (var inventoryStock in inventoryStocks)
        {
            var ingredientInventory = request.IngredientInventory.FirstOrDefault(x => x.IngredientId == inventoryStock.IngredientId.ToString());
            if (ingredientInventory != null)
            {
                if (inventoryStock.Quantity < (decimal) ingredientInventory.Quantity)
                {
                    _logger.Error("Insufficient stock for ingredient {IngredientInventoryIngredientId}", ingredientInventory.IngredientId);
                    response.IsValid = false;
                    response.InsufficientIngredientIds.Add(inventoryStock.IngredientId.ToString());
                    break;
                }
            }
            else
            {
                response.IsValid = false;
                _logger.Error("Ingredient {InventoryStockIngredientId} not found in request", inventoryStock.IngredientId);
                response.InsufficientIngredientIds.Add(inventoryStock.IngredientId.ToString());
            }
        }

        return response;
    }
}