using DimPos.Inventory.Domain.Models.Common;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.RollbackInventoryForOrder;

public class RollbackInventoryForOrderCommand : IRequest<ApiResponse>
{
    public Guid OrderId { get; set; }
}