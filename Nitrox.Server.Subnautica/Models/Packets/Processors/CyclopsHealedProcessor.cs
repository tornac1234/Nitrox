using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class CyclopsHealedProcessor : AuthenticatedPacketProcessor<CyclopsHealed>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public CyclopsHealedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(CyclopsHealed packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.CyclopsId, out VehicleEntity cyclopsEntity))
        {
            return;
        }

        if (cyclopsEntity.Metadata is CyclopsMetadata cyclopsMetadata)
        {
            cyclopsMetadata.Health = packet.NewHealth;
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
