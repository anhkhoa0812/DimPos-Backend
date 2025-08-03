using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Domain.Models.Response;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStockById;

public class GetInventoryStockByIdQueryHandler : IRequestHandler<GetInventoryStockByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    
    public GetInventoryStockByIdQueryHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetInventoryStockByIdQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }
        
        var inventoryStock = await _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.InventoryStockId && x.StoreId == storeId
        );
        if (inventoryStock == null)
        {
            throw new BadHttpRequestException("Không tìm thấy kho hàng");
        }
        var ingredients = await _catalogGrpcService.GetIngredientsByIngredientIdsAsync(
            new GetIngredientsByIngredientIdsRequest()
            {
                IngredientIds = { inventoryStock.IngredientId.ToString() }
            }
        );
        if (ingredients.Ingredients.Count == 0)
        {
            throw new BadHttpRequestException("Không tìm thấy nguyên liệu liên quan");
        }
        var ingredient = ingredients.Ingredients.FirstOrDefault();
        if (ingredient == null)
        {
            throw new BadHttpRequestException("Không tìm thấy nguyên liệu liên quan");
        }
        
        var response = new GetInventoryStockByIdResponse()
        {
            Id = inventoryStock.Id,
            Quantity = inventoryStock.Quantity,
            ReOrderLevel = inventoryStock.ReOrderLevel,
            CreatedDate = inventoryStock.CreatedDate,
            LastModifiedDate = inventoryStock.LastModifiedDate,
            Ingredient = new IngredientsForGetInventoryStockByIdResponse()
            {
                Id = Guid.Parse(ingredient.Id),
                Code = ingredient.Code,
                Name = ingredient.Name,
                Description = ingredient.Description,
                Sku = ingredient.Sku,
                MeasureUnit = ingredient.MeasureUnit
            }
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin kho hàng thành công",
            Data = response
        };
    }
}