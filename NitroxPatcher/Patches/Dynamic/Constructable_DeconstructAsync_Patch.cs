using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using System.Collections.Generic;
using System.Reflection;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic;

internal class Constructable_DeconstructAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Constructable t) => t.DeconstructAsync(default, default)));

    public static readonly InstructionsPattern InstructionsPattern = new()
    {
        Ldc_I4_0,
        Ret,
        Ldloc_1,
        { InstructionPattern.Call(nameof(Constructable), nameof(Constructable.UpdateMaterial)), "InsertDestruction" }
    };

    public static readonly List<CodeInstruction> InstructionsToAdd = new()
    {
        new(Ldloc_1),
        new(Call, Reflect.Method(() => Constructable_Construct_Patch.ConstructionAmountModified(default)))
    };

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) =>
        instructions.Transform(InstructionsPattern, (label, instruction) =>
        {
            if (label.Equals("InsertDestruction"))
            {
                return InstructionsToAdd;
            }
            return null;
        });

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
