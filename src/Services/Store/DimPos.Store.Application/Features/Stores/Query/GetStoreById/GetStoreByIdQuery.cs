using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStoreById;

public class GetStoreByIdQuery : IRequest<ApiResponse>
{
    public Guid StoreId { get; set; }
}