using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
public class CyclopsDamagePointEntity : Entity
{
    [DataMember(Order = 1)]
    public float Health { get; set; }

    [DataMember(Order = 2)]
    public int DamagePointIndex { get; set; }

    [DataMember(Order = 3)]
    public int FXPrefabIndex { get; set; }

    [IgnoreConstructor]
    protected CyclopsDamagePointEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsDamagePointEntity(float health, int damagePointIndex, int fxPrefabIndex, NitroxId entityId, NitroxId parentId)
    {
        Health = health;
        DamagePointIndex = damagePointIndex;
        FXPrefabIndex = fxPrefabIndex;

        Id = entityId;
        ParentId = parentId;
    }

    /// <remarks>Used for deserialization</remarks>
    public CyclopsDamagePointEntity(float health, int damagePointIndex, int fxPrefabIndex, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
    {
        Health = health;
        DamagePointIndex = damagePointIndex;
        FXPrefabIndex = fxPrefabIndex;
        Id = id;
        TechType = techType;
        Metadata = metadata;
        ParentId = parentId;
        ChildEntities = childEntities;
    }

    public override string ToString()
    {
        return $"[{nameof(CyclopsDamagePointEntity)} Health: {Health}, DamagePointIndex: {DamagePointIndex}, FXPrefabIndex: {FXPrefabIndex}]";
    }
}
