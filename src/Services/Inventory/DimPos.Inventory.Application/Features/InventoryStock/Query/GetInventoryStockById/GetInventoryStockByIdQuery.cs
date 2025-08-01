using DimPos.Inventory.Domain.Models.Common;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStockById;

public class GetInventoryStockByIdQuery : IRequest<ApiResponse>
{
    public Guid InventoryStockId { get; set; }
}