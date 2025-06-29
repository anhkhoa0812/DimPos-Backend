using DimPos.Store.Domain.Enums;
using DimPos.Store.Domain.Models.Common;
using Mediator;

namespace DimPos.Store.Application.Features.Stores.Command.CreateStore;

public class CreateStoreCommand : IRequest<ApiResponse>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? WifiName { get; set; }
    public string? WifiPassword { get; set; }
    public int? Index { get; set; }
    public string? LocalPasscode { get; set; }
    public string? ManagerName { get; set; }
    public EStoreType Type { get; set; }
    public decimal StartingStoreCashLending { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
