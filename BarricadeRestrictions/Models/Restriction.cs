using SeniorS.BarricadeRestrictions.Helpers;

namespace SeniorS.BarricadeRestrictions.Models;

public class Restriction
{
    public ushort Id { get; set; }

    [ConfigComment("If 0 don't allow any")]
    public int MaxPerUser { get; set; }

    [ConfigComment("This restriction affects placing in non vehicles?")]
    public bool AffectsNonVehicles { get; set; }
    
    [ConfigComment("This restriction affects placing in vehicles?")]
    public bool AffectsVehicles { get; set; }

    public Restriction(){}
    
    public Restriction(ushort id, int maxPerUser, bool affectsNonVehicles = true, bool affectsVehicles = false)
    {
        Id = id;
        MaxPerUser = maxPerUser;
        AffectsNonVehicles = affectsNonVehicles;
        AffectsVehicles = affectsVehicles;
    }
}