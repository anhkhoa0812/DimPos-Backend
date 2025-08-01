using DimPos.Inventory.Domain.Models.Common;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryTransaction.Query.GetInventoryTransactions;

public class GetInventoryTransactionsQuery : IRequest<ApiResponse>
{
    public Guid InventoryStockId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}