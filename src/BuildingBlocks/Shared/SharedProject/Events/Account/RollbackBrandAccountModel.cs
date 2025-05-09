using MassTransit;

namespace SharedProject.Events.Account;

public class RollbackBrandAccountModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; } 
    public Guid BrandId { get; set;  }
    public Guid AccountId { get; set;  }
} 