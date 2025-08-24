using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Dashboards.Query.GetDashboardForStore;

public class GetDashboardForStoreQuery : IRequest<ApiResponse>
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}