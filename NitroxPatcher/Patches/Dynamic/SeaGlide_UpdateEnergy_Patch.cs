using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents players from consuming other players' Seaglides energy.
/// </summary>
public sealed partial class Seaglide_UpdateEnergy_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Seaglide t) => t.UpdateEnergy());

    public static bool Prefix(Seaglide __instance)
    {
        // this value can only be set to the local player when using it
        // thus we only block this behaviour when local player isn't wielding the tool
        return __instance.usingPlayer;
    }
}
