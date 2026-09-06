using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class ToggleLightsMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Active { get; }

    [IgnoreConstructor]
    protected ToggleLightsMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ToggleLightsMetadata(bool active)
    {
        Active = active;
    }

    public override string ToString()
    {
        return $"[{nameof(ToggleLightsMetadata)} Active: {Active}]";
    }
}
