using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class UpdateInventoryForInternalOrderRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StorePurchaseOrderId { get; set; }
    public Guid StoreId { get; set; }
    public List<IngredientDetailsModel> IngredientDetailsModels { get; set; } = null!;
    
}