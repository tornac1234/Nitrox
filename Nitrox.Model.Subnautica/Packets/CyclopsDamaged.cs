using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CyclopsDamaged : Packet
{
    public NitroxId CyclopsId { get; }

    public float NewHealth { get; }

    public NitroxVector3 Position { get; }

    public DamageType DamageType { get; }

    public Optional<NitroxId> DealerId { get; }

    public CyclopsDamaged(NitroxId cyclopsId, float newHealth, NitroxVector3 position, DamageType damageType, Optional<NitroxId> dealerId)
    {
        CyclopsId = cyclopsId;
        NewHealth = newHealth;
        Position = position;
        DamageType = damageType;
        DealerId = dealerId;
    }
}
