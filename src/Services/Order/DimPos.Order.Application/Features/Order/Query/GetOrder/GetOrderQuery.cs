using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetOrder;

public class GetOrderQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
    public EOrderStatus? Status { get; set; }
    public EOrderType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}