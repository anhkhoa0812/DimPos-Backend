using MassTransit;

namespace SharedProject.Events.AssignMenuForStore;

public class AddStorePriceResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}