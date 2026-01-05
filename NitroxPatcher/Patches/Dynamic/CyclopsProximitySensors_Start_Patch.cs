using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces the piloting requirement to simply being in the Cyclops to see its proximity sensors.
/// </summary>
public sealed partial class CyclopsProximitySensors_Start_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsProximitySensors t) => t.Start());

    public static bool Prefix(CyclopsProximitySensors __instance)
    {
        SubRoot currentSubRoot = __instance.transform.parent.GetComponent<SubRoot>();
        Player.main.currentSubChangedEvent.AddHandler(__instance, (SubRoot subRoot) =>
        {
            // When you enter the Cyclops, we spoof the MB to think we're piloting it so it shows the UI
            __instance.OnPlayerModeChange(subRoot == currentSubRoot ? Player.Mode.Piloting : Player.Mode.Normal);
        });

        // As in base method
        __instance.uiWarningDot.ForEach(dot => dot.SetActive(false));

        return false;
    }
}
