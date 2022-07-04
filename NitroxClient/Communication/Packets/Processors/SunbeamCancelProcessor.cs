using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SunbeamCancelProcessor : ClientPacketProcessor<SunbeamCancel>
{
    public override void Process(SunbeamCancel packet)
    {
        StoryGoalCustomEventHandler main = StoryGoalCustomEventHandler.main;
        main.sunbeamCancel.delay = 0;
        main.sunbeamCancel.Trigger();
    }
}
