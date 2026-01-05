using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class CyclopsDamagedProcessor : ClientPacketProcessor<CyclopsDamaged>
{
    private readonly LiveMixinManager liveMixinManager;

    public CyclopsDamagedProcessor(LiveMixinManager liveMixinManager)
    {
        this.liveMixinManager = liveMixinManager;
    }

    public override void Process(CyclopsDamaged packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.CyclopsId, out LiveMixin cyclopsLiveMixin))
        {
            Log.ErrorOnce($"[{nameof(CyclopsDamagedProcessor)}] Could not find {nameof(LiveMixin)} with id {packet.CyclopsId}");
            return;
        }

        GameObject dealerObject = null;
        if (packet.DealerId.HasValue)
        {
            dealerObject = NitroxEntity.GetObjectFrom(packet.DealerId.Value).OrNull();
        }

        liveMixinManager.SyncRemoteHealth(cyclopsLiveMixin, packet.NewHealth, packet.Position.ToUnity(), packet.DamageType, dealerObject);
    }
}
