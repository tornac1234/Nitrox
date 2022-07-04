using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PDALogEntryAddProcessor : AuthenticatedPacketProcessor<PDALogEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaState;
        private readonly ScheduleKeeper scheduleKeeper;
        private readonly EventTriggerer eventTriggerer;

        public PDALogEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaState, ScheduleKeeper scheduleKeeper, EventTriggerer eventTriggerer)
        {
            this.playerManager = playerManager;
            this.pdaState = pdaState;
            this.scheduleKeeper = scheduleKeeper;
            this.eventTriggerer = eventTriggerer;
        }

        public override void Process(PDALogEntryAdd packet, Player player)
        {
            pdaState.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
            if (scheduleKeeper.ContainsScheduledGoal(packet.Key))
            {
                scheduleKeeper.UnScheduleGoal(packet.Key);
            }
            // This check is to make sure that the sunbeam update (sent by the client at the same time as this goal) reaches the server for sure
            if (string.Equals(packet.Key, "RadioSunbeam4", System.StringComparison.OrdinalIgnoreCase))
            {
                eventTriggerer.UpdateSunbeamState(true);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
