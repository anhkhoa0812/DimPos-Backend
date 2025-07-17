using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Query.GetStaffById;

public class GetStaffByIdQuery : IRequest<ApiResponse>
{
    public Guid StaffId { get; set; }
}