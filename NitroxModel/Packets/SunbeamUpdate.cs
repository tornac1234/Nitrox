using System;

namespace NitroxModel.Packets;

[Serializable]
public class SunbeamUpdate : Packet
{
    public bool CountdownActive { get; }
    public double? CountdownStartingTime { get; set; }
    public SunbeamUpdateType? UpdateType { get; set; }

    public SunbeamUpdate(bool countdownActive, double countdownStartingTime = 0)
    {
        CountdownActive = countdownActive;
        CountdownStartingTime = countdownStartingTime;
    }

    public SunbeamUpdate(SunbeamUpdateType updateType)
    {
        UpdateType = updateType;
    }

    // Same names as commands concerning Sunbeam in StoryGoalCustomEventHandler
    public enum SunbeamUpdateType
    {
        STARTSUNBEAMSTORY, PRECURSORGUNAIM, SUNBEAMCOUNTDOWNSTART
    }
}
