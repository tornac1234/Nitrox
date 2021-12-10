using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeSpeedCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly EventTriggerer eventTriggerer;

        public TimeSpeedCommand(PlayerManager playerManager, EventTriggerer eventTriggerer) : base("timespeed", Perms.ADMIN, "Changes the game time speed")
        {
            AddParameter(new TypeFloat("speed", true));

            this.playerManager = playerManager;
            this.eventTriggerer = eventTriggerer;
        }

        protected override void Execute(CallArgs args)
        {
            float speed = args.Get<float>(0);

            if (speed is < 0f or > 100f)
            {
                SendMessage(args.Sender, "Must specify value from 0 to 100.");
                return;
            }

            eventTriggerer.SetTimeSpeed(speed);
            playerManager.SendPacketToAllPlayers(new TimeSpeedChange(speed, eventTriggerer.ElapsedSeconds));
            Log.Info($"Setting day/night speed to {speed}");
        }
    }
}
