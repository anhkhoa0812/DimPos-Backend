using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Inventory.Infrastructure.Utils;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.UpdateQuantiyOfInventoryStock;

public class UpdateQuantityOfInventoryStockCommandHandler : IRequestHandler<UpdateQuantityOfInventoryStockCommand, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateQuantityOfInventoryStockCommandHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateQuantityOfInventoryStockCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");

        var inventoryStock = await _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.InventoryStockId && x.StoreId == storeId
        );
        if (inventoryStock == null)
            throw new BadHttpRequestException("Không tìm thấy kho hàng");
        
        if(inventoryStock.Quantity == request.Quantity)
            throw new BadHttpRequestException("Số lượng kho không thay đổi, không cần cập nhật");
        var oldQuantity = inventoryStock.Quantity;
        
        inventoryStock.Quantity = request.Quantity;
        inventoryStock.LastCountedAt = TimeUtil.GetCurrentSEATime();
        _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().UpdateAsync(inventoryStock);
        
        var newInventoryTransaction = new Domain.Entities.InventoryTransactions()
        {
            Id = Guid.CreateVersion7(),
            Type = EInventoryTransactionType.ManualAdjustment,
            QuantityChange = request.Quantity - oldQuantity,
            ReasonManualAdjustment = request.ReasonManualAdjustment,
            Note = request.Note,
            InventoryStockId = inventoryStock.Id,
        };
        await _unitOfWork.GetRepository<InventoryTransactions>().InsertAsync(newInventoryTransaction);

        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
            throw new BadHttpRequestException("Cập nhật số lượng kho thất bại");
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật số lượng kho thành công",
            Data = inventoryStock.Id
        };
    }
}