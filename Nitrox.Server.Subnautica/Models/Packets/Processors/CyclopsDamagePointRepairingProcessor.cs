using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class CyclopsDamagePointRepairingProcessor : AuthenticatedPacketProcessor<CyclopsDamagePointRepairing>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public CyclopsDamagePointRepairingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(CyclopsDamagePointRepairing packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.DamagePointId, out CyclopsDamagePointEntity cyclopsDamagePointEntity))
        {
            return;
        }

        // cyclops damage points's max health is 35
        if (packet.Health == 35f)
        {
            entityRegistry.RemoveEntity(packet.DamagePointId);
            Log.Debug("removed entity");
        }
        else
        {
            Log.Debug("set health");
            cyclopsDamagePointEntity.Health = packet.Health;
        }
        
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
