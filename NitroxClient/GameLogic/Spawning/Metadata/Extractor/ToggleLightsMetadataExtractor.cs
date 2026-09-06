using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ToggleLightsMetadataExtractor : EntityMetadataExtractor<ToggleLights, ToggleLightsMetadata>
{
    public override ToggleLightsMetadata Extract(ToggleLights toggleLights)
    {
        return new(toggleLights.lightsActive);
    }
}
