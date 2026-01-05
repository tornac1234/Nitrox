using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

public class CyclopsDamagePointRepairingProcessor : ClientPacketProcessor<CyclopsDamagePointRepairing>
{
    public override void Process(CyclopsDamagePointRepairing packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.DamagePointId, out CyclopsDamagePoint cyclopsDamagePoint))
        {
            Log.ErrorOnce($"[{nameof(CyclopsDamagePointRepairingProcessor)}] Could not find {nameof(CyclopsDamagePoint)} with id {packet.DamagePointId}");
            return;
        }

        Log.Debug($"processing {packet}");
        // TODO: fix global health not being updated

        cyclopsDamagePoint.liveMixin.health = packet.Health;
        if (cyclopsDamagePoint.liveMixin.IsFullHealth())
        {
            Log.Debug("full health");
            cyclopsDamagePoint.OnRepair();
            NitroxEntity.RemoveFrom(cyclopsDamagePoint.gameObject);
        }
    }
}
