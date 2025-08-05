using MassTransit;

namespace SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

public class CreateOrderResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StoreId { get; set; }
    public Guid OrderId { get; set; }
    public List<IngredientForUpdateInventoryModel> Ingredients { get; set; } = new List<IngredientForUpdateInventoryModel>();
}