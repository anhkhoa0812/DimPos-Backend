namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class StorePurchaseOrderItemRequestModel
{
    public Guid ProductVariantId { get; set; }
    public decimal ApprovedQuantityByBrand { get; set; }
}