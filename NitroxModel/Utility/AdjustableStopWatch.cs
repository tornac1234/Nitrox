using System;
using System.Diagnostics;

namespace NitroxModel.Utility
{
    public class AdjustableStopWatch : Stopwatch
    {
        private float speed;
        internal long amplifiedElapsedMilliseconds;
        private long lastElapsedMilliseconds;

        public float Speed
        {
            get => speed;
            set
            {
                GetElapsedMilliseconds();
                speed = value;
            }
        }

        [Obsolete("Elapsed isn't supported in AdjustableStopWatch.")]
        public new long Elapsed => throw new NotSupportedException();

        [Obsolete("ElapsedTicks isn't supported in AdjustableStopWatch.")]
        public new long ElapsedTicks => throw new NotSupportedException();

        public new long ElapsedMilliseconds => GetElapsedMilliseconds();

        private long GetElapsedMilliseconds()
        {
            amplifiedElapsedMilliseconds += (long)((base.ElapsedMilliseconds - lastElapsedMilliseconds) * Speed);
            lastElapsedMilliseconds = base.ElapsedMilliseconds;
            return amplifiedElapsedMilliseconds;
        }

        ///<summary>Stops time interval measurement, resets the elapsed time to zero and sets the speed to 1.</summary>
        public new void Reset()
        {
            base.Reset();
            lastElapsedMilliseconds = 0L;
            amplifiedElapsedMilliseconds = 0L;
            speed = 1L;
        }

        ///<summary>Stops time interval measurement, resets the elapsed time to zero, keeps the defined speed and starts measuring elapsed time.</summary>
        public new void Restart()
        {
            base.Restart();
            lastElapsedMilliseconds = 0L;
            amplifiedElapsedMilliseconds = 0L;
        }

        public new void Stop()
        {
            base.Stop();
            speed = 1L;
        }
    }
}
