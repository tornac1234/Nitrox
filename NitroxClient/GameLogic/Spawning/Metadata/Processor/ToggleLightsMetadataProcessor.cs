using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class ToggleLightsMetadataProcessor : EntityMetadataProcessor<ToggleLightsMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, ToggleLightsMetadata metadata)
    {
        if (gameObject.TryGetComponent(out ToggleLights toggleLights))
        {
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            using (FMODSystem.SuppressSendingSounds())
            {
                toggleLights.SetLightsActive(metadata.Active);
            }
        }
        else
        {
            Log.Error($"[{nameof(ToggleLightsMetadataProcessor)}] Could not find {nameof(ToggleLights)} on {gameObject.name}");
        }
    }
}
