using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;

public class Upgrade_V1900(ILogger<Upgrade_V1900> logger) : SaveDataUpgrade(logger)
{
    public override Version TargetVersion { get; } = new(1, 9, 0, 0);

    protected override void UpgradeEntityData(JObject data)
    {
        RefactorFlashlightMetadata(data);
    }

    protected override void UpgradeGlobalRootData(JObject data)
    {
        RefactorFlashlightMetadata(data);
    }

    private static void RefactorFlashlightMetadata(JObject data)
    {
        string flashLightMetadataType = "Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata.FlashlightMetadata, Nitrox.Model.Subnautica";
        string toggleLightsMetadataType = "Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata.ToggleLightsMetadata, Nitrox.Model.Subnautica";

        List<JProperty> flashlightTypeProperties = [.. data.DescendantsAndSelf().OfType<JProperty>().Where(p => p.Name == "$type" && (string)p.Value == flashLightMetadataType)];

        foreach (JProperty typeProperty in flashlightTypeProperties)
        {
            if (typeProperty.Parent is JObject parentObject)
            {
                typeProperty.Value = toggleLightsMetadataType;
                JProperty onProperty = parentObject.Property("On");
                parentObject.Add("Active", onProperty.Value);
                onProperty.Remove();
            }
        }
    }
}
