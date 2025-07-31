namespace SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

public class IngredientForUpdateInventoryModel
{
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
}