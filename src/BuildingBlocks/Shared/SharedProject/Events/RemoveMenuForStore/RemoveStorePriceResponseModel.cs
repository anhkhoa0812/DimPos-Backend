using MassTransit;

namespace SharedProject.Events.RemoveMenuForStore;

public class RemoveStorePriceResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}