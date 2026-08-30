using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Register <see cref="WalkBehaviour"/> movements at <see cref="EntityPositionBroadcaster"/> for future broadcast.
/// </summary>
public sealed partial class WalkBehaviour_GoToInternal_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WalkBehaviour t) => t.GoToInternal(default, default, default));

    public static void Prefix(WalkBehaviour __instance, Vector3 targetPosition, Vector3 targetDirection, float velocity)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId entityId))
        {
            EntityPositionBroadcaster.Instance.RegisterLocomotionChange(entityId, __instance.gameObject, targetPosition, targetDirection, velocity);
        }
    }
}
