using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Watches for any modification to the sunbeam, and if any, notifies the server of it
/// </summary>
public class StoryGoalCustomEventHandler_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalCustomEventHandler t) => t.NotifyGoalComplete(default));

    /// <summary>
    /// Decide whether or not we want the events (concerning sunbeam) to be triggered. Instead, we may want to ask the server
    /// </summary>
    public static bool Prefix(StoryGoalCustomEventHandler __instance, string key)
    {
        switch (key.ToLower())
        {
            case "OnPlayRadioSunbeam4":
                // We don't want this event to play before the server syncs everyone to do it
                Resolve<IPacketSender>().Send(new SunbeamUpdate(true));
                return false;
            case "Goal_Disable_Gun":
                // We don't need to cancel this case because it's just an acknowledgement of the cancellation
                if (StoryGoalManager.main.pendingRadioMessages.Any(message => string.Equals(message, "RadioSunbeam4", System.StringComparison.OrdinalIgnoreCase)))
                {
                    Resolve<IPacketSender>().Send(new SunbeamCancel());
                }
                break;
            case "SunbeamCheckPlayerRange":
                // in this case, we only want to notice the server that the countdown is no longer active
                Resolve<IPacketSender>().Send(new SunbeamUpdate(false));
                break;
        }
        // In the case the event is just a sunbeam event, we need to verify if we ever trigger the sunbeamCancel
        if (__instance.sunbeamGoals.Any(goal => string.Equals(key, goal.trigger, System.StringComparison.OrdinalIgnoreCase)) &&
            __instance.gunDisabled)
        {
            Resolve<IPacketSender>().Send(new SunbeamCancel());
            return false;
        }

        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
