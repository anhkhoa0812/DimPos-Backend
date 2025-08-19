using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.TaxRate.Query.GetTaxRateByStoreId;

public class GetTaxRateByStoreIdQuery : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}