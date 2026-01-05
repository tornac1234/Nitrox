using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Fire_Douse_Patch : NitroxPatch, IDynamicPatch
{
    //public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.Douse(default));

    public static void Postfix(Fire __instance, float amount)
    {
        // TODO: fill those
        if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
        {

        }
        else
        {

        }
    }
}
