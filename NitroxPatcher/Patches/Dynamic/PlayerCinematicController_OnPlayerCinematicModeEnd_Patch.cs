using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    // private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.OnPlayerCinematicModeEnd());

    public static void Prefix(PlayerCinematicController __instance)
    {
        if (!__instance.cinematicModeActive)
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity))
        {
            Log.Warn($"[{nameof(PlayerCinematicController_OnPlayerCinematicModeEnd_Patch)}] - No NitroxEntity for \"{__instance.GetFullHierarchyPath()}\" found!");
            return;
        }

        int identifier = MultiplayerCinematicReference.GetCinematicControllerIdentifier(__instance.gameObject, entity.gameObject);
        Resolve<PlayerCinematics>().EndCinematicMode(Resolve<IMultiplayerSession>().Reservation.PlayerId, entity.Id, identifier, __instance.playerViewAnimationName);
    }
}
