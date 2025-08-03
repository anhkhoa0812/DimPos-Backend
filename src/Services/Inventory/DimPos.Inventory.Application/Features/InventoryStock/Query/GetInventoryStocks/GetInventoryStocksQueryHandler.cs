using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Domain.Models.Response;
using DimPos.Inventory.Infrastructure.Paginate;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStocks;

public class GetInventoryStocksQueryHandler : IRequestHandler<GetInventoryStocksQuery, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    
    public GetInventoryStocksQueryHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService, CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetInventoryStocksQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if(storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var inventoryStocks = await _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().GetPagingListAsync(
            predicate: x => x.StoreId == storeId,
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        var ingredientIds = inventoryStocks.Items.Select(x => x.IngredientId.ToString()).Distinct().ToList();

        var ingredientFromGrpc = await _catalogGrpcService.GetIngredientsByIngredientIdsAsync(
            new GetIngredientsByIngredientIdsRequest()
            {
                IngredientIds = { ingredientIds }
            }
        );
        var response = new List<GetInventoryStocksResponse>();
        foreach (var inventoryStock in inventoryStocks.Items)
        {
            var ingredient = ingredientFromGrpc.Ingredients.First(x => x.Id == inventoryStock.IngredientId.ToString());
            response.Add(new GetInventoryStocksResponse()
            {
                Id = inventoryStock.Id,
                Quantity = inventoryStock.Quantity,
                ReOrderLevel = inventoryStock.ReOrderLevel,
                CreatedDate = inventoryStock.CreatedDate,
                LastModifiedDate = inventoryStock.LastModifiedDate,
                Ingredient = new IngredientsForGetInventoryStocksResponse()
                {
                    Id = Guid.Parse(ingredient.Id),
                    Code = ingredient.Code,
                    Name = ingredient.Name,
                    Description = ingredient.Description,
                    Sku = ingredient.Sku,
                    MeasureUnit = ingredient.MeasureUnit
                }
            });
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách tồn kho thành công",
            Data = new Paginate<GetInventoryStocksResponse>()
            {
                Page = inventoryStocks.Page,
                Size = inventoryStocks.Size,
                Total = inventoryStocks.Total,
                TotalPages = inventoryStocks.TotalPages,
                Items = response
            }
        };

    }
}