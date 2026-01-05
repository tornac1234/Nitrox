using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ensures only the player simulating a Cyclops can and will broadcast the creation of damage points for it.
/// </summary>
public sealed partial class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsExternalDamageManager t) => t.CreatePoint());

    public static bool Prefix(CyclopsExternalDamageManager __instance)
    {
        return __instance.subRoot.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id);
    }

    /*
     * this.unusedDamagePoints.RemoveAt(num);
     * CyclopsExternalDamageManager_CreatePoint_Patch.BroadcastPointCreated(this, num, gameObject);   [INSERTED LINED]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldfld),
                                                new CodeMatch(OpCodes.Ldloc_0),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((List<CyclopsDamagePoint> t) => t.RemoveAt(default))),
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Ldloc_1),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastPointCreated(default, default, default))),
                                            ]).InstructionEnumeration();
    }

    public static void BroadcastPointCreated(CyclopsExternalDamageManager cyclopsExternalDamageManager, int unusedIndex, GameObject fxPrefabObject)
    {
        if (!cyclopsExternalDamageManager.subRoot.TryGetNitroxId(out NitroxId subRootId))
        {
            return;
        }

        CyclopsDamagePoint damagePoint = cyclopsExternalDamageManager.unusedDamagePoints[unusedIndex];

        int damagePointIndex = cyclopsExternalDamageManager.damagePoints.GetIndex(damagePoint);
        int fxPrefabIndex = cyclopsExternalDamageManager.fxPrefabs.GetIndex(fxPrefabObject);

        NitroxId pointId = NitroxEntity.GenerateNewId(damagePoint.gameObject);

        // Value 1f comes from CyclopsDamagePoint.RestoreHealth
        CyclopsDamagePointEntity cyclopsDamagePointEntity = new(1f, damagePointIndex, fxPrefabIndex, pointId, subRootId);
        Resolve<Entities>().BroadcastEntitySpawnedByClient(cyclopsDamagePointEntity);
    }
}
