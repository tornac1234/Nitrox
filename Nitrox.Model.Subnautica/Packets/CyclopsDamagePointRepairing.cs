using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CyclopsDamagePointRepairing : Packet
{
    public NitroxId DamagePointId { get; }
    public float Health { get; }

    public CyclopsDamagePointRepairing(NitroxId damagePointId, float health)
    {
        DamagePointId = damagePointId;
        Health = health;
    }

    public override string ToString()
    {
        return $"[{nameof(CyclopsDamagePointRepairing)} DamagePointId: {DamagePointId}, Health: {Health}]";
    }
}
