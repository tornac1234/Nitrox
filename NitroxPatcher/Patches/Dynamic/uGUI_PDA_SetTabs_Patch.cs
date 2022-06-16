﻿using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
{
    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.SetTabs(default));

    public static bool Prefix(uGUI_PDA __instance, List<PDATab> tabs)
    {
        int num = (tabs != null) ? tabs.Count : 0;
        Atlas.Sprite[] array = new Atlas.Sprite[num];
        __instance.currentTabs.Clear();
        for (int i = 0; i < num; i++)
        {
            PDATab item = tabs[i];
            array[i] = SpriteManager.Get(SpriteManager.Group.Tab, string.Format("Tab{0}", item.ToString()));
            
            __instance.currentTabs.Add(item);
        }
        // the last tab is the one we added in uGUI_PDA_Initialize_Patch
        if (HasBundleLoaded(NitroxAssetBundle.PLAYER_LIST_TAB))
        {
            List<NitroxPDATab> customTabs = new(Resolve<NitroxGuiManager>().CustomTabs.Values);
            for (int i = 0; i < customTabs.Count; i++)
            {
                string tabIconAssetName = customTabs[customTabs.Count - i - 1].TabIconAssetName;
                array[array.Length - i - 1] = AssetsHelper.MakeAtlasSpriteFromTexture(tabIconAssetName);
            }
        }
        else
        {
            // As a placeholder, we use the normal player icon
            array[num - 1] = array[0];
            AssetsHelper.onPlayerListAssetsLoaded += () => { AssignSprite(__instance.toolbar); };
        }

        uGUI_Toolbar uGUI_Toolbar = __instance.toolbar;
        object[] content = array;
        uGUI_Toolbar.Initialize(__instance, content, null, 15);
        __instance.CacheToolbarTooltips();
        return false;
    }

    private static void AssignSprite(uGUI_Toolbar uGUI_Toolbar)
    {
        // Last is player list tab's one
        List<NitroxPDATab> customTabs = new(Resolve<NitroxGuiManager>().CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            string tabIconAssetName = customTabs[customTabs.Count - i - 1].TabIconAssetName;
            uGUI_Toolbar.icons[uGUI_Toolbar.icons.Count - i - 1].SetForegroundSprite(AssetsHelper.MakeAtlasSpriteFromTexture(tabIconAssetName));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
