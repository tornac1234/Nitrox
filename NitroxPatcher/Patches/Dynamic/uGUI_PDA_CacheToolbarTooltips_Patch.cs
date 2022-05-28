﻿using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Provide Subnautica with the new PDA tabs' names
/// </summary>
public class uGUI_PDA_CacheToolbarTooltips_Patch : NitroxPatch, IDynamicPatch
{
    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.CacheToolbarTooltips());

    public static void Postfix(uGUI_PDA __instance)
    {
        // Modify the latest tooltips of the list, which are the ones for the newly created tab
        List<NitroxPDATab> customTabs = new(Resolve<NitroxGuiManager>().CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            /* considering a list like: [a,b,c,d,e,f] (toolbarTooltips)
             * We want to modify only the n (customTabs.Count) last elements to replace with
             * the elements from the list [u,v,w] (customTabs)
             * we start from the end of the list (toolbarTooltips.Count) and remove,
             * not i but n - i, because we want to have the right order
             */
            int index = i + __instance.toolbarTooltips.Count - (customTabs.Count - i);
            __instance.toolbarTooltips[index] = TooltipFactory.Label(customTabs[i].ToolbarTip());
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
