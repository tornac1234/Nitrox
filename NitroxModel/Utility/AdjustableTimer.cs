using System.Timers;
using NitroxModel.Logger;

namespace NitroxModel.Utility
{
    public class AdjustableTimer : Timer
    {
        public float Speed = 1f;
        private double intervalBackup;
        private long lastElapsedMilliseconds;

        public void SetSpeed(float newSpeed, long elapsedMilliseconds)
        {
            if (Speed == newSpeed)
            {
                return;
            }
            if (newSpeed == 0f)
            {
                base.Stop();
            }
            else if (newSpeed == 1f)
            {
                RecoverInterval(elapsedMilliseconds);
            }
            else
            {
                if (Speed == 0f)
                {
                    Start();
                }
                else if (Speed != 1f)
                {
                    RecoverInterval(elapsedMilliseconds);
                    lastElapsedMilliseconds = elapsedMilliseconds;
                }
                intervalBackup = Interval - (elapsedMilliseconds - lastElapsedMilliseconds);
                SetInterval(intervalBackup / newSpeed);
            }
            lastElapsedMilliseconds = elapsedMilliseconds;
            Speed = newSpeed;
        }

        private void RecoverInterval(long elapsedMilliseconds)
        {
            double delta = intervalBackup - (elapsedMilliseconds - lastElapsedMilliseconds);
            SetInterval(delta);
        }

        public new void Stop()
        {
            base.Stop();
            Speed = 1f;
        }

        public void SetInterval(double newInterval)
        {
            if (newInterval < 1)
            {
                Interval = 1;
            }
            else if (newInterval > int.MaxValue)
            {
                Interval = int.MaxValue;
                Log.Warn("Tried to set an interval greater than int.MaxValue");
            }
            else
            {
                Interval = newInterval;
            }
        }
    }
}
