using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    class DayNightCycle_OnConsoleCommand_daynightspeed_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.OnConsoleCommand_daynightspeed(default(NotificationCenter.Notification)));

        // The command is skipped on the client because it's up to the server to control the time speed
        public static bool Prefix(NotificationCenter.Notification n)
        {
            if (n.data.Count > 0 && float.TryParse((string)n.data[0], out float speed))
            {
                Resolve<IPacketSender>().Send(new ServerCommand("timespeed " + speed));
            }
            else
            {
                ErrorMessage.AddDebug("Must specify value from 0 to 100.");
            }
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
