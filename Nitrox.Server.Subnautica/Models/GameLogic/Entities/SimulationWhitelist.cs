using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal static class SimulationWhitelist
{
    /// <summary>
    ///     We don't want to give out simulation to all entities that the server sent out because there is a lot of stationary items and junk (TechType.None).
    ///     It is easier to maintain a list of items we should simulate than try to blacklist items. This list should not be checked for non-server spawned items
    ///     as they were probably dropped by the player and are mostly guaranteed to move.
    /// </summary>
    public static readonly HashSet<NitroxTechType> MovementWhitelist =
    [
        TechType.Biter.ToDto(),
        TechType.Bladderfish.ToDto(),
        TechType.Blighter.ToDto(),
        TechType.Bloom.ToDto(),
        TechType.BlueAmoeba.ToDto(),
        TechType.BoneShark.ToDto(),
        TechType.Boomerang.ToDto(),
        TechType.CaveCrawler.ToDto(),
        TechType.Constructor.ToDto(),
        TechType.CrabSquid.ToDto(),
        TechType.Crabsnake.ToDto(),
        TechType.Crash.ToDto(),
        TechType.Cutefish.ToDto(),
        TechType.Eyeye.ToDto(),
        TechType.Floater.ToDto(),
        TechType.GarryFish.ToDto(),
        TechType.Gasopod.ToDto(),
        TechType.GhostLeviathan.ToDto(),
        TechType.GhostLeviathanJuvenile.ToDto(),
        TechType.GhostRayBlue.ToDto(),
        TechType.GhostRayRed.ToDto(),
        TechType.HoleFish.ToDto(),
        TechType.Hoopfish.ToDto(),
        TechType.Hoverfish.ToDto(),
        TechType.Jellyray.ToDto(),
        TechType.Jumper.ToDto(),
        TechType.LargeFloater.ToDto(),
        TechType.LargeKoosh.ToDto(),
        TechType.LavaBoomerang.ToDto(),
        TechType.LavaEyeye.ToDto(),
        TechType.LavaLarva.ToDto(),
        TechType.LavaLizard.ToDto(),
        TechType.Mesmer.ToDto(),
        TechType.Oculus.ToDto(),
        TechType.Peeper.ToDto(),
        TechType.PipeSurfaceFloater.ToDto(),
        TechType.RabbitRay.ToDto(),
        TechType.ReaperLeviathan.ToDto(),
        TechType.Reefback.ToDto(),
        TechType.Reginald.ToDto(),
        TechType.RockPuncher.ToDto(),
        TechType.Rockgrub.ToDto(),
        TechType.Sandshark.ToDto(),
        TechType.SeaDragon.ToDto(),
        TechType.SeaEmperor.ToDto(),
        TechType.SeaEmperorBaby.ToDto(),
        TechType.SeaEmperorJuvenile.ToDto(),
        TechType.SeaEmperorLeviathan.ToDto(),
        TechType.SeaTreader.ToDto(),
        TechType.Shocker.ToDto(),
        TechType.Shuttlebug.ToDto(),
        TechType.Skyray.ToDto(),
        TechType.Spadefish.ToDto(),
        TechType.SpineEel.ToDto(),
        TechType.Spinefish.ToDto(),
        TechType.Stalker.ToDto(),
        TechType.Warper.ToDto()
    ];

    /// <summary>
    ///     We differentiate the entities which should be simulated because of one of their behaviour (ie for utility)
    ///     from those are simulated for their movements.
    /// </summary>
    public static readonly HashSet<NitroxTechType> UtilityWhitelist = new()
    {
        TechType.CrashHome.ToDto()
    };
}
