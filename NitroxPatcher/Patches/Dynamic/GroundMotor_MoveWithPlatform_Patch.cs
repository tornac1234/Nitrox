using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class GroundMotor_MoveWithPlatform_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((GroundMotor t) => t.SetEnabled(default));

    public static void Prefix(GroundMotor __instance, bool enabled)
    {
        if (enabled && Player.main.currentSub && Player.main.currentSub.isCyclops)
        {
            __instance.movingPlatform.movementTransfer = GroundMotor.MovementTransferOnJump.PermaLocked;
        }
        else
        {
            __instance.movingPlatform.movementTransfer = GroundMotor.MovementTransferOnJump.PermaTransfer;
        }
    }
}
