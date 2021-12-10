using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class TimeSpeedChangeProcessor : ClientPacketProcessor<TimeSpeedChange>
    {
        public override void Process(TimeSpeedChange packet)
        {
            if (DayNightCycle.main)
            {
                DayNightCycle.main.timePassedAsDouble = packet.CurrentTime;
                DayNightCycle.main._dayNightSpeed = packet.Speed;
                DayNightCycle.main.skipTimeMode = false;
                ErrorMessage.AddDebug($"Setting day/night speed to {packet.Speed}.");
                Log.Info($"Processed a TimeSpeedChange packet [Speed: {packet.Speed}]");
            }
        }
    }
}
