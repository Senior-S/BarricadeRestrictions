using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SeniorS.BarricadeRestrictions.Helpers;
using SeniorS.BarricadeRestrictions.Models;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SeniorS.BarricadeRestrictions;

public class BarricadeRestrictions : RocketPlugin<Configuration>
{
    public static BarricadeRestrictions Instance;

    protected override void Load()
    {
        Instance = this;
        Configuration.Instance.LoadComments();

        BarricadeManager.onDeployBarricadeRequested += OnDeployBarricadeRequested;
        
        Logger.Log($"BarricadeRestrictions v{this.Assembly.GetName().Version}");
        Logger.Log("<<SSPlugins>>");
    }

    private void OnDeployBarricadeRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angleX, 
        ref float angleY, ref float angleZ, ref ulong owner, ref ulong group, ref bool shouldAllow)
    {
        RocketPlayer player = new(owner.ToString());
        if (player.HasPermission(Instance.Configuration.Instance.bypassLimitPermission) 
            || (Instance.Configuration.Instance.shouldAdminBypassLimit && player.IsAdmin)) return;
        
        if (Instance.Configuration.Instance.restrictions.All(c => c.Id != asset.id)) return;
        Restriction restriction = Instance.Configuration.Instance.restrictions.First(c => c.Id == asset.id);
        
        // Users are dumb so let's avoid possible expensive calculations if the restriction isn't properly set up
        if (!restriction.AffectsVehicles && !restriction.AffectsNonVehicles) return;
        UnturnedPlayer user = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
        string formatedException = restriction.MaxPerUser > 0 ? MessageHelper.FormatMessage("generic_limit", restriction.MaxPerUser)
            : MessageHelper.FormatMessage("total_restriction");
        
        int barricadesInVehicles = BarricadeManager.vehicleRegions.Sum(c => c.drops.Count(d => d.asset.id == restriction.Id));
        List<BarricadeDrop> drops = BarricadeManager.regions.Cast<BarricadeRegion>().SelectMany(region => region.drops).ToList();
        int barricadesInNonVehicles = drops.Count(c => !c.IsChildOfVehicle && c.asset.id == restriction.Id);

        if (restriction.AffectsVehicles && restriction.AffectsNonVehicles)
        {
            int allBarricades = barricadesInVehicles + barricadesInNonVehicles;
            if (allBarricades < restriction.MaxPerUser && restriction.MaxPerUser != 0) return;
        
            shouldAllow = false;
            MessageHelper.Say(user.SteamPlayer(), "restriction_hit_generic", true, formatedException);
            return;
        }

        bool isVehicle = false;
        if (hit != null)
        {
            isVehicle = VehicleManager.vehicles.Any(c => c.transform.GetInstanceID() == hit.GetInstanceID());
        }
        
        if (!restriction.AffectsVehicles && restriction.AffectsNonVehicles)
        {
            if (isVehicle || (barricadesInNonVehicles < restriction.MaxPerUser && restriction.MaxPerUser != 0)) return;
            
            shouldAllow = false;
            MessageHelper.Say(user.SteamPlayer(), "restriction_hit_non_vehicles", true, formatedException);
            return;
        }

        if (!isVehicle) return;

        if (!restriction.AffectsVehicles || restriction.AffectsNonVehicles) return;
        if (barricadesInVehicles < restriction.MaxPerUser && restriction.MaxPerUser != 0) return;
        
        shouldAllow = false;

        MessageHelper.Say(user.SteamPlayer(), "restriction_hit_vehicles", true, formatedException);
    }

    public override TranslationList DefaultTranslations => new()
    {
        { "prefix", "-=color=#FFC312=-[-=/color=--=color=#009432=-Restrictions-=/color=--=color=#FFC312=-]-=/color=-" },
        { "generic_limit", "more than {0}" },
        { "total_restriction", "any" },
        { "restriction_hit_generic", "Oops, you can't place {0} of this barricade" },
        { "restriction_hit_vehicles", "Oops, you can't place {0} of this barricade on vehicles!" },
        { "restriction_hit_non_vehicles", "Oops, you can't place {0} of this barricade on non vehicles!" }
    };

    protected override void Unload()
    {
        Instance = null;
        
        BarricadeManager.onDeployBarricadeRequested -= OnDeployBarricadeRequested;

        Logger.Log("<<SSPlugins>>");
    }
}