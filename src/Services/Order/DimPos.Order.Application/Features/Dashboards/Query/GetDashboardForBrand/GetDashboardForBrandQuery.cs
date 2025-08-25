using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Dashboards.Query.GetDashboardForBrand;

public class GetDashboardForBrandQuery : IRequest<ApiResponse>
{
    public Guid? StoreId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}