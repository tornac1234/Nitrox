using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Creature_ChooseBestAction_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Creature t) => t.ChooseBestAction(default));

    public static bool Prefix(Creature __instance, out NitroxId __state, ref CreatureAction __result)
    {
        if (!__instance.TryGetIdOrWarn(out __state, true))
        {
            Log.WarnOnce($"[{nameof(Creature_ChooseBestAction_Patch)}] Couldn't find an id on {__instance.GetFullHierarchyPath()}");
            return true;
        }
        if (Resolve<SimulationOwnership>().HasAnyLockType(__state))
        {
            return true;
        }

        // If we have received any order
        if (Resolve<AI>().TryGetActionForCreature(__instance, out CreatureAction action))
        {
            __result = action;
        }
        return false;
    }

    public static void Postfix(Creature __instance, bool __runOriginal, NitroxId __state, ref CreatureAction __result)
    {
        if (!__runOriginal || __state == null)
        {
            return;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(__state))
        {
            Resolve<AI>().BroadcastNewAction(__state, __instance, __result);
        }
    }
}
