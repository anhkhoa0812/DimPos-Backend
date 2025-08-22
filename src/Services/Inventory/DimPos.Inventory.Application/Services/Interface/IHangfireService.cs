namespace DimPos.Inventory.Application.Services.Interface;

public interface IHangfireService
{
    Task CheckReOrderLevelAsync();
}