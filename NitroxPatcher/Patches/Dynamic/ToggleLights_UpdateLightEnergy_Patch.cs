using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents players from consuming other players' Seaglides and Flashlights energy for light.
/// </summary>
public sealed partial class ToggleLights_UpdateLightEnergy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ToggleLights t) => t.UpdateLightEnergy());

    public static bool Prefix(ToggleLights __instance)
    {
        // ToggleLights are either on the Seaglide or on the FlashLight
        if (__instance.TryGetComponent(out PlayerTool playerTool))
        {
            // this value can only be set to the local player when using it
            // thus we only block this behaviour when local player isn't wielding the tool
            return playerTool.usingPlayer;
        }

        // ToggleLights can also be on a SeaMoth's child
        // in this case, it doesn't consume energy so no need to check for simulation ownership
        return true;
    }
}
