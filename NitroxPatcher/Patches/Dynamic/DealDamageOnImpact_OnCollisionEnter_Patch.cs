using System.Reflection;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents damage between vehicles since their collisions are highly bugged and could provoke instant death sometimes.
/// </summary>
public sealed partial class DealDamageOnImpact_OnCollisionEnter_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((DealDamageOnImpact t) => t.OnCollisionEnter(default));

    public static bool Prefix(DealDamageOnImpact __instance, Collision collision)
    {
        if (Resolve<LiveMixinManager>().IsVehicleOrCyclops(__instance) && Resolve<LiveMixinManager>().IsVehicleOrCyclops(collision.gameObject))
        {
            return false;
        }

        return true;
    }
}
