using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using Story;
using static NitroxModel.Packets.SunbeamUpdate;

namespace NitroxClient.Communication.Packets.Processors;

public class SunbeamUpdateProcessor : ClientPacketProcessor<SunbeamUpdate>
{
    private List<string> sunbeamEventKeys;

    public SunbeamUpdateProcessor()
    {
        sunbeamEventKeys = new()
        {
            StoryGoalCustomEventHandler.main.gunDeactivate.key,
            StoryGoalCustomEventHandler.main.sunbeamCancel.key,
            StoryGoalCustomEventHandler.main.sunbeamDestroyEventInRange.key,
            StoryGoalCustomEventHandler.main.sunbeamDestroyEventOutOfRange.key,
            "SunbeamCheckPlayerRange",
            "PrecursorGunAimCheck",
            "RadioSunbeamStart",
            "OnPlayRadioSunbeam4"
        };

        foreach (StoryGoalCustomEventHandler.SunbeamGoal sunbeamGoal in StoryGoalCustomEventHandler.main.sunbeamGoals)
        {
            sunbeamEventKeys.Add(sunbeamGoal.key);
        }
        Log.Debug($"Sunbeam Event Keys {string.Join(", ", sunbeamEventKeys)}");
    }

    public override void Process(SunbeamUpdate packet)
    {
        StoryGoalCustomEventHandler main = StoryGoalCustomEventHandler.main;
        // 1. We only want to update the countdown if it's the packet's content
        if (packet.CountdownStartingTime.HasValue)
        {
            main.countdownActive = packet.CountdownActive;
            main.countdownStartingTime = (float)packet.CountdownStartingTime.Value;
            return;
        }
        // 2. Else, it means the packet is filled with an update type which we want to treat
        // In the case of the three commands, we can simply call them back
        // 3. A specificity of Nitrox is we want the events to happen again even if they already happened, therefore we need to remove the goals from the completedGoals list
        // Because when StoryGoalManager.main.OnGoalComplete, it doesn't trigger the event if it was already completed
        ResetSunbeamEvents();
        switch (packet.UpdateType)
        {
            case SunbeamUpdateType.STARTSUNBEAMSTORY:
                StoryGoalCustomEventHandler.main.OnConsoleCommand_startsunbeamstoryevent();
                break;
            case SunbeamUpdateType.PRECURSORGUNAIM:
                StoryGoalCustomEventHandler.main.OnConsoleCommand_precursorgunaim();
                break;
            case SunbeamUpdateType.SUNBEAMCOUNTDOWNSTART:
                StoryGoalCustomEventHandler.main.OnConsoleCommand_sunbeamcountdownstart();
                break;
        }
    }

    /// <summary>
    /// Reset the state of everything that was modified when any of the sunbeam events was triggered
    /// </summary>
    private void ResetSunbeamEvents()
    {
        foreach (string key in sunbeamEventKeys)
        {
            bool removed = StoryGoalManager.main.completedGoals.Remove(key);
            Log.Debug($"Removed completed goal {key} : {removed}");
        }
        CompoundGoalTracker compoundGoalTracker = StoryGoalManager.main.compoundGoalTracker;
        // Repopulate the elements in a list from where they were removed once they've already played
        // When they're triggered at least once
        foreach (CompoundGoal compoundGoal in compoundGoalTracker.goalData.goals)
        {
            Log.Debug($"CompoundGoal {compoundGoal.key}, delay: {compoundGoal.delay}");
            if (sunbeamEventKeys.Contains(compoundGoal.key) && !compoundGoalTracker.goals.Contains(compoundGoal))
            {
                compoundGoalTracker.goals.Add(compoundGoal);
            }
        }
    }
}
