using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class TimeSpeedChange : Packet
    {
        public float Speed { get; }
        public double CurrentTime { get; }

        public TimeSpeedChange(float speed, double currentTime)
        {
            Speed = speed;
            CurrentTime = currentTime;
        }
    }
}
