using System;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.Utility;

namespace NitroxServer.GameLogic
{
    public class EventTriggerer
    {
        internal readonly Dictionary<string, AdjustableTimer> eventTimers = new();
        internal readonly AdjustableStopWatch stopWatch = new();
        private readonly PlayerManager playerManager;

        public readonly double AuroraExplosionTime;
        internal double elapsedTimeOutsideStopWatch;
        private float TimeSpeed;

        public double ElapsedTime
        {
            get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatch;
            private set => elapsedTimeOutsideStopWatch = value - stopWatch.ElapsedMilliseconds;
        }

        public double ElapsedSeconds
        {
            get => ElapsedTime * 0.001;
            private set => ElapsedTime = value * 1000;
        }

        public EventTriggerer(PlayerManager playerManager, double elapsedTime, double? auroraExplosionTime)
        {
            this.playerManager = playerManager;
            TimeSpeed = 1f;
            elapsedTimeOutsideStopWatch = elapsedTime;

            Log.Debug($"Event Triggerer started! ElapsedTime={Math.Floor(ElapsedSeconds)}s");


            if (auroraExplosionTime.HasValue)
            {
                AuroraExplosionTime = auroraExplosionTime.Value;
            }
            else
            {
                AuroraExplosionTime = RandomNumber(2.3d, 4d) * 1200d * 1000d; //Time.deltaTime returns seconds so we need to multiply 1000
            }

            CreateTimer(AuroraExplosionTime * 0.2d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateTimer(AuroraExplosionTime * 0.5d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateTimer(AuroraExplosionTime * 0.8d - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateTimer(AuroraExplosionTime - ElapsedTime, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateTimer(AuroraExplosionTime - ElapsedTime, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");

            stopWatch.Start();
        }

        private void CreateTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            if (time <= 0) // Ignoring if time is in the past
            {
                return;
            }

            AdjustableTimer timer = new()
            {
                Interval = time,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += delegate
            {
                eventTimers.Remove(key);
                Log.Info($"Triggering event type {eventType} at time {time} with param {key}");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };

            eventTimers.Add(key, timer);
        }

        private double RandomNumber(double min, double max)
        {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        public void StartWorldTime()
        {
            stopWatch.Start();
        }

        public void PauseWorldTime()
        {
            stopWatch.Stop();
        }

        public void ResetWorldTime()
        {
            stopWatch.Reset();
        }

        public void StartEventTimers()
        {
            foreach (AdjustableTimer eventTimer in eventTimers.Values)
            {
                eventTimer.Start();
            }
        }

        public void PauseEventTimers()
        {
            foreach (AdjustableTimer eventTimer in eventTimers.Values)
            {
                eventTimer.Stop();
            }
        }

        public void SetTimeSpeed(float speed)
        {
            stopWatch.Speed = speed;
            foreach (AdjustableTimer timer in eventTimers.Values)
            {
                timer.SetSpeed(speed, stopWatch.ElapsedMilliseconds);
            }
        }

        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTime += 1200000.0 - ElapsedTime % 1200000.0 + 600000.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTime += 1200000.0 - ElapsedTime % 1200000.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTime += 600000.0 - ElapsedTime % 600000.0;
                    break;
            }

            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds, false));
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
