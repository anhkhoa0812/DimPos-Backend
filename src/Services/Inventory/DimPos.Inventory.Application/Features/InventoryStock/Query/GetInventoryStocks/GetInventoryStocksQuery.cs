using DimPos.Inventory.Domain.Models.Common;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Query.GetInventoryStocks;

public class GetInventoryStocksQuery : IRequest<ApiResponse>
{
    public int Page { get; set; } 
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}