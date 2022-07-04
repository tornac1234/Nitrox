using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Helper;
using NitroxServer.GameLogic.Unlockables;
using static NitroxModel.Packets.SunbeamUpdate;

namespace NitroxServer.GameLogic
{
    /// <summary>
    /// Keeps track of Aurora-related events
    /// </summary>
    public class EventTriggerer
    {
        internal readonly Dictionary<string, Timer> eventTimers = new();
        private readonly Stopwatch stopWatch = new();
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;
        private readonly StoryGoalData storyGoalData;
        private readonly SunbeamData sunbeamData;
        private string seed;

        public double AuroraExplosionTimeMs;
        // Necessary to calculate the timers correctly
        public double AuroraWarningTimeMs;

        public Dictionary<string, SunbeamEvent> SunbeamEvents = new();
        public double SunbeamEndTimeMs => sunbeamData.CountdownStartingTimeMs + 2400f * 1000f;
        public Timer SunbeamExplosionTimer;

        private double elapsedTimeOutsideStopWatchMs;

        /// <summary>
        /// Total Elapsed Time in milliseconds
        /// </summary>
        public double ElapsedTimeMs
        {
            get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatchMs;
            internal set
            {
                foreach (Timer timer in eventTimers.Values)
                {
                    timer.Interval = Math.Max(1, timer.Interval - (value - ElapsedTimeMs));
                }
                elapsedTimeOutsideStopWatchMs = value - stopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Total Elapsed Time in seconds
        /// </summary>
        public double ElapsedSeconds
        {
            get => ElapsedTimeMs * 0.001;
            private set => ElapsedTimeMs = value * 1000;
        }

        /// <summary>
        /// Subnautica's equivalent of days
        /// </summary>
        // Using ceiling because days count start at 1 and not 0
        public int Day => (int)Math.Ceiling(ElapsedTimeMs / TimeSpan.FromMinutes(20).TotalMilliseconds);

        public EventTriggerer(PlayerManager playerManager, PDAStateData pdaStateData, StoryGoalData storyGoalData, SunbeamData sunbeamData, string seed, double elapsedTime, double? auroraExplosionTime, double? auroraWarningTime)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;
            this.sunbeamData = sunbeamData;
            this.seed = seed;

            FillSunbeamEvents();
            // Default time in Base SN is 480s
            elapsedTimeOutsideStopWatchMs = elapsedTime == 0 ? TimeSpan.FromSeconds(480).TotalMilliseconds : elapsedTime;
            AuroraExplosionTimeMs = auroraExplosionTime ?? GenerateDeterministicAuroraTime(seed);
            AuroraWarningTimeMs = auroraWarningTime ?? ElapsedTimeMs;
            CreateAuroraEventTimers();
            CreateSunbeamEventTimer();
            stopWatch.Start();
            Log.Debug($"Event Triggerer started! ElapsedTime={Math.Floor(ElapsedSeconds)}s");
            Log.Debug($"Aurora will explode in {GetMinutesBeforeAuroraExplosion()} minutes");
        }

        /// <summary>
        /// Creates every timer that will keep track of the time before we need to trigger an event
        /// </summary>
        private void CreateAuroraEventTimers()
        {
            double ExplosionCycleDuration = AuroraExplosionTimeMs - AuroraWarningTimeMs;
            // If aurora's warning is set to later than explosion's time, we don't want to create any timer
            if (ExplosionCycleDuration < 0)
            {
                return;
            }
            double TimePassedSinceWarning = ElapsedTimeMs - AuroraWarningTimeMs;
            CreateSunbeamTimer(ExplosionCycleDuration * 0.2d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateSunbeamTimer(ExplosionCycleDuration * 0.5d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateSunbeamTimer(ExplosionCycleDuration * 0.8d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateSunbeamTimer(ExplosionCycleDuration - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateSunbeamTimer(ExplosionCycleDuration - TimePassedSinceWarning, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");
        }

        /// <summary>
        /// Updates saved sunbeam's countdown activity only if it's different than the previously registered one
        /// </summary>
        public void UpdateSunbeamState(bool countdownActive)
        {
            // We want to ignore any packet we would have received twice
            if (sunbeamData.CountdownActive == countdownActive)
            {
                return;
            }

            // We update local information
            sunbeamData.CountdownActive = countdownActive;
            
            // And conveniently treat the information according the provided state of the countdown
            SunbeamUpdate sunbeamUpdate = new(countdownActive);
            if (countdownActive)
            {
                sunbeamData.CountdownStartingTimeMs = (float)ElapsedTimeMs;
                // On client-side, the countdown is in seconds
                sunbeamUpdate.CountdownStartingTime = sunbeamData.CountdownStartingTimeMs * 0.001;
                playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds));
                CreateSunbeamEventTimer();
            }
            else
            {
                if (SunbeamExplosionTimer != null)
                {
                    SunbeamExplosionTimer.Stop();
                    SunbeamExplosionTimer.Dispose();
                }
            }

            playerManager.SendPacketToAllPlayers(sunbeamUpdate);
        }

        /// <summary>
        /// Create a timer for the sunbeam explosion event only if the countdown is already active
        /// </summary>
        private void CreateSunbeamEventTimer()
        {
            // We only want to create the timer if the cooldown is active
            if (!sunbeamData.CountdownActive)
            {
                return;
            }
            double time = SunbeamEndTimeMs - ElapsedTimeMs;
            Log.Debug($"Creating sunbeam even timer with time {time}");

            TryCreateTimerWithCallback(time, delegate ()
            {
                Log.Info($"Triggering event at time {time} with param PrecursorGunAimCheck");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(StoryEventSend.EventType.STORY, "PrecursorGunAimCheck"));
                UpdateSunbeamState(false);
                SunbeamExplosionTimer = null;
            }, out SunbeamExplosionTimer);
        }

        private void FillSunbeamEvents()
        {
            SunbeamEvents["RadioSunbeam1"] = new(0, StoryEventSend.EventType.RADIO, "RadioSunbeam1", "RadioSunbeamStart");
            SunbeamEvents["RadioSunbeam2"] = new(600, StoryEventSend.EventType.RADIO, "RadioSunbeam2", "OnPlayRadioSunbeam1");
            SunbeamEvents["RadioSunbeam3"] = new(2400, StoryEventSend.EventType.RADIO, "RadioSunbeam3", "OnPlayRadioSunbeam2");
            SunbeamEvents["RadioSunbeam4"] = new(2400, StoryEventSend.EventType.RADIO, "RadioSunbeam4", "OnPlayRadioSunbeam3");
            SunbeamEvents["PrecursorGunAim"] = new(0, StoryEventSend.EventType.STORY, "PrecursorGunAim", "PrecursorGunAimCheck");

            SunbeamEvents["SunbeamCancel"] = new(2400, StoryEventSend.EventType.RADIO, "RadioSunbeamCancel");
            SunbeamEvents["PDASunbeamDestroyEventInRange"] = new(0, StoryEventSend.EventType.PDA, "PDASunbeamDestroyEventInRange");
            SunbeamEvents["PDASunbeamDestroyEventOutOfRange"] = new(22, StoryEventSend.EventType.PDA, "PDASunbeamDestroyEventOutOfRange");
            SunbeamEvents["Goal_Disable_Gun"] = new(0, StoryEventSend.EventType.STORY, "Goal_Disable_Gun");
        }

        private void SendSunbeamCancel()
        {
            SunbeamEvent sunbeamCancel = SunbeamEvents["SunbeamCancel"];
            playerManager.SendPacketToAllPlayers(new StoryEventSend(sunbeamCancel.GoalType, sunbeamCancel.Key));
            Log.Info($"Triggering event type {sunbeamCancel.GoalType} at time {ElapsedTimeMs} with param {sunbeamCancel.Key}");
        }

        /// <summary>
        /// When starting the server, if some events already happened, the time parameter will be &lt; 0
        /// in which case we don't want to create the timer
        /// </summary>
        /// <param name="time">In milliseconds</param>
        private void CreateSunbeamTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            if (TryCreateTimerWithCallback(time, delegate ()
            {
                eventTimers.Remove(key);
                Log.Info($"Triggering event type {eventType} at time {time} with param {key}");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            }, out Timer timer))
            {
                Log.Debug($"Created sunbeam timer, it will explode in {time}ms");
                eventTimers.Add(key, timer);
            }
        }

        private bool TryCreateTimerWithCallback(double time, Action action, out Timer timer)
        {
            // If time is not valid, we just want to make sure that the timer will still be created
            if (time <= 0)
            {
                timer = new Timer();
                return false;
            }

            timer = new()
            {
                Interval = time,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += delegate
            {
                action();
            };
            return true;
        }

        /// <summary>
        /// Tells the players to start Aurora's explosion event
        /// </summary>
        /// <param name="cooldown">Wether we should make Aurora explode instantly or after a short countdown</param>
        public void ExplodeAurora(bool cooldown)
        {
            ClearTimers();
            AuroraExplosionTimeMs = ElapsedTimeMs;
            // Explode aurora with a cooldown is like default game just before aurora is about to explode
            if (cooldown)
            {
                // These lines should be filled with the same informations as in the constructor
                playerManager.SendPacketToAllPlayers(new StoryEventSend(StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4"));
                playerManager.SendPacketToAllPlayers(new StoryEventSend(StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion"));
                Log.Info("Started Aurora's explosion sequence");
            }
            else
            {
                // This will make aurora explode instantly on clients
                playerManager.SendPacketToAllPlayers(new AuroraExplodeNow());
                Log.Info("Exploded Aurora");
            }
        }

        /// <summary>
        /// Tells the players to start Aurora's restoration event
        /// </summary>
        public void RestoreAurora()
        {
            ClearTimers();
            AuroraExplosionTimeMs = GenerateDeterministicAuroraTime(seed) + ElapsedTimeMs;
            AuroraWarningTimeMs = ElapsedTimeMs;
            CreateAuroraEventTimers();
            // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
            foreach (string timerKey in eventTimers.Keys)
            {
                PDALogEntry logEntry = pdaStateData.PdaLog.Find(entry => entry.Key == timerKey);
                // Wether or not we found the entry doesn't matter
                pdaStateData.PdaLog.Remove(logEntry);
                storyGoalData.CompletedGoals.Remove(timerKey);
            }
            playerManager.SendPacketToAllPlayers(new AuroraRestore());
            Log.Info($"Restored Aurora, will explode again in {GetMinutesBeforeAuroraExplosion()} minutes");
        }

        /// <summary>
        /// Starts the sunbeam timer and notifies every connected client
        /// </summary>
        public void StartSunbeamCountdown()
        {
            // TODO: clean the events that only play once
            // The SunbeamUpdate packet with the SunbeamUpdateType must be sent first because it contains important information that must be processed before the following one
            // Explanation can be found there: SunbeamUpdateProcessor.cs (NitroxClient)
            playerManager.SendPacketToAllPlayers(new SunbeamUpdate(SunbeamUpdateType.SUNBEAMCOUNTDOWNSTART));
            UpdateSunbeamState(true);
        }

        /// <summary>
        /// Tells every connected client to start the precursor gun aim cinematic
        /// </summary>
        public void PrecursorGunAim()
        {
            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds));
            playerManager.SendPacketToAllPlayers(new SunbeamUpdate(SunbeamUpdateType.PRECURSORGUNAIM));
        }

        /// <summary>
        /// Tells every client to start the sunbeam story and to remove any trace of sunbeam radio goals they could have
        /// </summary>
        public void StartSunbeamStory()
        {
            // TODO: Remove sunbeam radio goals from saved list and notify the client

            // storyGoalData.CompletedGoals.RemoveAll()
            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds));
            playerManager.SendPacketToAllPlayers(new SunbeamUpdate(SunbeamUpdateType.STARTSUNBEAMSTORY));
        }

        /// <summary>
        /// Removes every timer that's still alive
        /// </summary>
        private void ClearTimers()
        {
            foreach (Timer timer in eventTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            eventTimers.Clear();
        }

        /// <summary>
        /// Calculate the future Aurora's explosion time in a deterministic manner
        /// </summary>
        private double GenerateDeterministicAuroraTime(string seed)
        {
            // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
            DeterministicGenerator generator = new(seed, nameof(EventTriggerer));
            return elapsedTimeOutsideStopWatchMs + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
        }

        /// <summary>
        /// Restarts every event timer
        /// </summary>
        public void StartWorld()
        {
            stopWatch.Start();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Start();
            }
        }

        /// <summary>
        /// Pauses every event timer
        /// </summary>
        public void PauseWorld()
        {
            stopWatch.Stop();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Stop();
            }
        }

        /// <summary>
        /// Calculates the time before the aurora explosion
        /// </summary>
        /// <returns>The time in minutes before aurora explodes or -1 if it already exploded</returns>
        private double GetMinutesBeforeAuroraExplosion()
        {
            return AuroraExplosionTimeMs > ElapsedTimeMs ? Math.Round((AuroraExplosionTimeMs - ElapsedTimeMs) / 60000) : -1;
        }

        /// <summary>
        /// Makes a nice status for the summary command for example
        /// </summary>
        public string GetAuroraStateSummary()
        {
            double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
            if (minutesBeforeExplosion < 0)
            {
                return "already exploded";
            }
            string stateNumber = "";
            if (eventTimers.Count > 0)
            {
                // Event timer events should always have a number at last character
                string nextEventKey = eventTimers.ElementAt(0).Key;
                stateNumber = $" [{nextEventKey[nextEventKey.Length - 1]}/4]";
            }
            
            return $"explodes in {minutesBeforeExplosion} minutes{stateNumber}";
        }

        public bool GunDisabled()
        {
            return storyGoalData.CompletedGoals.Contains("Goal_Disable_Gun");
        }

        internal void ResetWorld()
        {
            stopWatch.Reset();
        }

        /// <summary>
        /// Set current time (replication of SN's system)
        /// </summary>
        /// <param name="type">Type of the operation to apply</param>
        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0 + 600000.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTimeMs += 600000.0 - ElapsedTimeMs % 600000.0;
                    break;
            }

            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds));
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }

        public class SunbeamEvent
        {
            public int Delay;
            public StoryEventSend.EventType GoalType;
            public string Key;
            public string Trigger;

            public SunbeamEvent(int delay, StoryEventSend.EventType goalType, string key, string trigger) : this(delay, goalType, key)
            {
                Trigger = trigger;
            }

            public SunbeamEvent(int delay, StoryEventSend.EventType goalType, string key) : base()
            {
                Delay = delay;
                GoalType = goalType;
                Key = key;
            }
        }
    }
}
