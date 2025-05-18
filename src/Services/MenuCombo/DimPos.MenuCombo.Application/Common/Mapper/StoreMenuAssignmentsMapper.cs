using DimPos.MenuCombo.Domain.Entities;
using Riok.Mapperly.Abstractions;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.MenuCombo.Application.Common.Mapper;

[Mapper]
public static partial class StoreMenuAssignmentsMapper
{
    
    public static partial StoreMenuAssignments Map(StoreMenuAssignmentsModel source);
    public static partial StoreMenuItemAvailability Map(StoreMenuItemAvailabilityModel source);
    public static partial StoreMenuAssignmentsModel Map(StoreMenuAssignments source);
    public static partial StoreMenuItemAvailabilityModel Map(StoreMenuItemAvailability source);
    public static partial List<StoreMenuItemAvailability> Map(List<StoreMenuItemAvailabilityModel> source);
    
    public static partial List<StoreMenuItemAvailabilityModel> Map(List<StoreMenuItemAvailability> source);
    
    public static List<StoreMenuAssignments> Map(List<StoreMenuAssignmentsModel> sources)
    {
        var response  = new List<StoreMenuAssignments>();
        foreach (var source in sources)
        {
            var storeMenuAssignments = Map(source);
            storeMenuAssignments.StoreMenuItemAvailability = Map(source.StoreMenuItemAvailability);
            response.Add(storeMenuAssignments);
        }

        return response;
    }
    public static List<StoreMenuAssignmentsModel> Map(List<StoreMenuAssignments> sources)
    {
        var response  = new List<StoreMenuAssignmentsModel>();
        foreach (var source in sources)
        {
            var storeMenuAssignmentModel = Map(source);
            if (source.StoreMenuItemAvailability != null)
                storeMenuAssignmentModel.StoreMenuItemAvailability = Map(source.StoreMenuItemAvailability.ToList());
        }
        return response;
    }
}