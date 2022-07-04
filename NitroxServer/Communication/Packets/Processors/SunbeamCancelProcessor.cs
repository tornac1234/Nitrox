using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

/// <summary>
/// Forwards the sunbeam cancel event trigger to other players
/// </summary>
public class SunbeamCancelProcessor : AuthenticatedPacketProcessor<SunbeamCancel>
{
    private readonly PlayerManager playerManager;

    public SunbeamCancelProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(SunbeamCancel packet, Player player)
    {
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
