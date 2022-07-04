using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// (TODO) Centralize every command execution to prevent them from happening (switch the authority to the server)
/// </summary>
public class NotificationCenter_PostNotification_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((NotificationCenter t) => t.PostNotification(default));

    // Temporary stuff, to remove when making the actual system
    private static Dictionary<string, string> commandsToPatch = new() { { "startsunbeamstoryevent", "story" }, { "precursorgunaim", "gunaim" }, { "sunbeamcountdownstart", "countdown" } };

    public static bool Prefix(NotificationCenter.Notification aNotification)
    {
        if (commandsToPatch.TryGetValue(aNotification.name.Replace("OnConsoleCommand_", ""), out string command))
        {
            Resolve<IPacketSender>().Send(new ServerCommand($"sunbeam {command}"));
            return false;
        }

        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
