using MassTransit;

namespace SharedProject.Events.Brand;

public class CreateBrandAccountErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public Guid AccountId { get; set; }
}