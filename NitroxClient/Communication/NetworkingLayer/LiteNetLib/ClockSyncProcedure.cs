using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib;

public sealed class ClockSyncProcedure(LiteNetLibClient liteNetLibClient) : IDisposable
{
    private readonly LiteNetLibClient liteNetLibClient = liteNetLibClient;
    private readonly int previousPingInterval = liteNetLibClient.PingInterval;
    private readonly List<long> deltas = [];

    public static ClockSyncProcedure Start(LiteNetLibClient liteNetLibClient, int procedureDuration)
    {
        ClockSyncProcedure clockSyncProcedure = new(liteNetLibClient);
        liteNetLibClient.PingInterval = procedureDuration * 1000;
        liteNetLibClient.LatencyUpdateCallback += clockSyncProcedure.LatencyUpdate;
        return clockSyncProcedure;
    }

    public void LatencyUpdate(long remoteTimeDelta)
    {
        Log.Debug($"rtd: {remoteTimeDelta}");
        deltas.Add(remoteTimeDelta);
        liteNetLibClient.ForceUpdate();
    }

    /// <summary>
    /// Get an average of remote delta times gathered until now without the ones which are not in the standard deviation range.
    /// </summary>
    public bool TryGetSafeAverageRTD(out long average)
    {
        if (deltas.Count == 0)
        {
            Log.Debug("0 deltas");
            average = 0;
            return false; // abnormal situation
        }

        average = (long)deltas.Average();
        Log.Debug($"average 1: {average}");

        // manual calculation of standard deviation
        long standardDeviation = 0;

        // sum of the squares of the values
        foreach (long delta in deltas)
        {
            standardDeviation += delta * delta;
        }
        standardDeviation /= deltas.Count; // divide by n

        standardDeviation -= average * average; // remove the average's square

        standardDeviation = (long)Math.Sqrt(standardDeviation);
        Log.Debug($"std: {standardDeviation}");

        List<long> validValues = [];
        foreach (long delta in deltas)
        {
            if (Math.Abs(delta - average) <= standardDeviation)
            {
                validValues.Add(delta);
            }
        }

        if (validValues.Count == 0)
        {
            Log.Debug($"0 valid values");
            return true; // value is not really meaningful ...
        }

        average = (long)validValues.Average();
        return true;
    }

    public void Dispose()
    {
        liteNetLibClient.LatencyUpdateCallback -= LatencyUpdate;
        liteNetLibClient.PingInterval = previousPingInterval;
    }
}