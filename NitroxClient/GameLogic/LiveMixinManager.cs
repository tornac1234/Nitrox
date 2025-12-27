using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class LiveMixinManager
{
    private readonly SimulationOwnership simulationOwnership;
    private static readonly HashSet<string> broadcastDeathClassIdWhitelist = new()
    {
        "7d307502-46b7-4f86-afb0-65fe8867f893" // Crash (fish)
    };

    public bool IsRemoteHealthChanging { get; private set; }

    public LiveMixinManager(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;
    }

    public bool IsVehicleOrCyclops(GameObject gameObject)
    {
        if (gameObject.GetComponent<Vehicle>())
        {
            return true;
        }

        return gameObject.TryGetComponent(out SubRoot subRoot) && subRoot.isCyclops;
    }

    public bool IsVehicleOrCyclops(Component component)
    {
        return IsVehicleOrCyclops(component.gameObject);
    }

    public bool ShouldAddHealth(LiveMixin liveMixin)
    {
        if (IsRemoteHealthChanging)
        {
            return true;
        }

        // Currently, we only apply live mixin updates to vehicles as there is more work to implement
        // damage for regular entities like fish.
        if (!IsVehicleOrCyclops(liveMixin))
        {
            return true;
        }

        // As a general rule, for unexpected objects (no NitroxId) we don't block anything to avoid any impact on the user experience
        if (!liveMixin.TryGetNitroxId(out NitroxId nitroxId))
        {
            return true;
        }

        return simulationOwnership.HasAnyLockType(nitroxId);
    }
    
    public bool ShouldTakeDamage(LiveMixin victimLiveMixin, GameObject dealer)
    {
        if (IsRemoteHealthChanging)
        {
            return true;
        }

        // Same reason as in ShouldAddHealth
        if (!IsVehicleOrCyclops(victimLiveMixin))
        {
            return true;
        }

        // When there's a damage dealer, the only priority is to check simulation ownership on it
        if (dealer)
        {
            if (!dealer.TryGetIdOrWarn(out NitroxId dealerId))
            {
                return true;
            }

            return simulationOwnership.HasAnyLockType(dealerId);
        }

        if (!victimLiveMixin.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        return simulationOwnership.HasAnyLockType(id);
    }

    public bool ShouldBroadcastDeath(LiveMixin liveMixin)
    {
        if (liveMixin.TryGetComponent(out UniqueIdentifier uniqueIdentifier) && !string.IsNullOrEmpty(uniqueIdentifier.classId))
        {
            return broadcastDeathClassIdWhitelist.Contains(uniqueIdentifier.classId);
        }

        return true;
    }

    public void SyncRemoteHealth(LiveMixin liveMixin, float remoteHealth, Vector3 position = default, DamageType damageType = DamageType.Normal)
    {
        if (liveMixin.health == remoteHealth)
        {
            return;
        }

        float difference = remoteHealth - liveMixin.health;

        IsRemoteHealthChanging = true;

        // We catch the exceptions here because we don't want IsRemoteHealthChanging to be stuck to true
        try
        {
            if (difference < 0)
            {
                liveMixin.TakeDamage(difference, position, damageType);
            }
            else
            {
                liveMixin.AddHealth(difference);
            }
        } catch (Exception e)
        {
            Log.Error(e, $"Encountered an exception while processing health update");
        }

        IsRemoteHealthChanging = false;

        // We mainly only do the above to trigger damage effects and sounds.  After those, we sync the remote value
        // to ensure that any floating point discrepencies aren't an issue.
        liveMixin.health = remoteHealth;
    }
}
