using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class CyclopsHealedProcessor : ClientPacketProcessor<CyclopsHealed>
{
    private readonly LiveMixinManager liveMixinManager;

    public CyclopsHealedProcessor(LiveMixinManager liveMixinManager)
    {
        this.liveMixinManager = liveMixinManager;
    }

    public override void Process(CyclopsHealed packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.CyclopsId, out LiveMixin cyclopsLiveMixin))
        {
            Log.ErrorOnce($"[{nameof(CyclopsDamagedProcessor)}] Could not find {nameof(LiveMixin)} with id {packet.CyclopsId}");
            return;
        }

        liveMixinManager.SyncRemoteHealth(cyclopsLiveMixin, packet.NewHealth);
    }
}
