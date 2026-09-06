using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts <see cref="Seaglide"/>'s light toggle by EntityMetadataUpdate.
/// </summary>
public sealed partial class Seaglide_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Seaglide t) => t.onLightsToggled(default));

    public static void Prefix(Seaglide __instance, bool active)
    {
        if (__instance.usingPlayer && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            ToggleLightsMetadataExtractor flashlightMetadataExtractor = Resolve<ToggleLightsMetadataExtractor>();
            ToggleLightsMetadata flashlightMetadata = flashlightMetadataExtractor.Extract(__instance.toggleLights);
            Resolve<Entities>().BroadcastMetadataUpdate(id, flashlightMetadata);
        }
    }
}
