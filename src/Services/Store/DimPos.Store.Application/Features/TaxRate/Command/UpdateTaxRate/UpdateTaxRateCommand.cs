using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Command.UpdateTaxRate;

public class UpdateTaxRateCommand : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public Guid TaxRateId { get; set; }
    public string? Name { get; set; } 
    public decimal? Rate { get; set; }
    public bool? IsActive { get; set; }
}
public class UpdateTaxRateRequest
{
    public string? Name { get; set; } 
    public decimal? Rate { get; set; }
    public bool? IsActive { get; set; }
}