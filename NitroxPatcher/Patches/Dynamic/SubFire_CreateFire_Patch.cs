using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubFire_CreateFire_Patch : NitroxPatch, IDynamicPatch
{
    //private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubFire t) => t.CreateFire(default(SubFire.RoomFire)));

    public static bool Prefix(SubFire __instance, SubFire.RoomFire startInRoom)
    {
        // TODO: fill this

        return __instance.subRoot.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id);
    }
}
