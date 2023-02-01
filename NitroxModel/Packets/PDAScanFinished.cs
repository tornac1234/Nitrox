using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class PDAScanFinished : Packet
{
    public NitroxId Id { get; }
    public NitroxTechType TechType { get; }
    public int UnlockedAmount { get; }
    public bool FullyResearched { get; }
    public bool Destroy { get; }

    public PDAScanFinished(NitroxId id, NitroxTechType techType,  int unlockedAmount, bool fullyResearched, bool destroy)
    {
        Id = id;
        TechType = techType;
        UnlockedAmount = unlockedAmount;
        FullyResearched = fullyResearched;
        Destroy = destroy;
    }
}
