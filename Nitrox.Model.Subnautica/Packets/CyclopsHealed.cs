using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CyclopsHealed : Packet
{
    public NitroxId CyclopsId { get; }
    public float NewHealth { get; }

    public CyclopsHealed(NitroxId cyclopsId, float newHealth)
    {
        CyclopsId = cyclopsId;
        NewHealth = newHealth;
    }

    public override string ToString()
    {
        return $"[{nameof(CyclopsHealed)} CyclopsId: {CyclopsId}, NewHealth: {NewHealth}]";
    }
}
