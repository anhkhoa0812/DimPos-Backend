using DimPos.Inventory.Domain.Models.Common;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.UpdateQuantiyOfInventoryStock;

public class UpdateQuantityOfInventoryStockCommand : IRequest<ApiResponse>
{
    public Guid InventoryStockId { get; set; }
    public decimal Quantity { get; set; }
    public string ReasonManualAdjustment { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class UpdateQuantityOfInventoryStockRequest
{
    public decimal Quantity { get; set; }
    public string ReasonManualAdjustment { get; set; } = string.Empty;
    public string? Note { get; set; }
}