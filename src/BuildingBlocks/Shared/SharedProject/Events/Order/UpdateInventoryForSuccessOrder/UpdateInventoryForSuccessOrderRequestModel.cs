namespace SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

public class UpdateInventoryForSuccessOrderRequestModel
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StoreId { get; set; }
    public Guid OrderId { get; set; }
    public List<IngredientForUpdateInventoryModel> Ingredients { get; set; } = new List<IngredientForUpdateInventoryModel>();
}