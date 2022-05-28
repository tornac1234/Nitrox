﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Decide whether or not we want to render the pings on the screen
/// </summary>
public class uGUI_Pings_OnWillRenderCanvases_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_Pings t) => t.OnWillRenderCanvases());

    private static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
    private static readonly object INJECTION_OPERAND = Reflect.Method((uGUI_Pings t) => t.IsVisibleNow());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);

        List<CodeInstruction> instructionList = instructions.ToList();

        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];
            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                /*
                 * Replace
                 * bool flag = this.IsVisibleNow();
                 * by
                 * bool flag = uGUI_Pings_OnWillRenderCanvases_Patch.IsVisible(this);
                 */
                instruction.operand = Reflect.Method(() => IsVisible(default));
            }
            yield return instruction;
        }
    }

    private static bool IsVisible(uGUI_Pings t)
    {
        if (t.IsVisibleNow())
        {
            return true;
        }
        if (!uGUI_PDA.main)
        {
            return false;
        }
        return Resolve<NitroxGuiManager>().CustomTabs.TryGetValue(uGUI_PDA.main.currentTabType, out NitroxPDATab nitroxTab) && nitroxTab.KeepPingsVisible();
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
