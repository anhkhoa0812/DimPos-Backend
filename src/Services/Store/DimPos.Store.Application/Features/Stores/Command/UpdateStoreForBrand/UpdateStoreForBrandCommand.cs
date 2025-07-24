using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStoreForBrand;

public class UpdateStoreForBrandCommand : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
    public EStoreStatus? Status { get; set; }
    public decimal? StartingStoreCashLending { get; set; }
}
public class UpdateStoreForBrandRequest
{
    public EStoreStatus? Status { get; set; }
    public decimal? StartingStoreCashLending { get; set; }
}