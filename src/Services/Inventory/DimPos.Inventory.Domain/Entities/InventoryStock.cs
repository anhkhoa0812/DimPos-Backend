using DimPos.Inventory.Domain.Entities.Common;

namespace DimPos.Inventory.Domain.Entities;

public class InventoryStock : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReOrderLevel { get; set; }
    public virtual ICollection<InventoryTransactions> InventoryTransactions { get; set; } = new List<InventoryTransactions>();
}