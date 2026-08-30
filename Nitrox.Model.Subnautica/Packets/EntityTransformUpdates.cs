using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class EntityTransformUpdates(List<EntityTransformUpdates.EntityTransformUpdate> updates) : Packet
{
    public List<EntityTransformUpdate> Updates { get; } = updates;

    public override string ToString()
    {
        return $"[{nameof(EntityTransformUpdates)}: {string.Join(" ", Updates)} ]";
    }

    [Serializable]
    public abstract class EntityTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
    {
        public NitroxId Id { get; } = id;
        public NitroxVector3 Position { get; } = position;
        public NitroxQuaternion Rotation { get; } = rotation;
    }

    [Serializable]
    public class RawTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation) : EntityTransformUpdate(id, position, rotation)
    {
        public override string ToString()
        {
            return $"[{nameof(RawTransformUpdate)} Id: {Id}, Position: {Position}, Rotation: {Rotation}]";
        }
    }

    [Serializable]
    public class LocomotionUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 destinationPosition, NitroxVector3 destinationDirection, float velocity) : EntityTransformUpdate(id, position, rotation)
    {
        public NitroxVector3 DestinationPosition { get; } = destinationPosition;
        public NitroxVector3 DestinationDirection { get; } = destinationDirection;
        public float Velocity { get; } = velocity;

        public override string ToString()
        {
            return $"[{nameof(LocomotionUpdate)} Id: {Id}, Position: {Position}, Rotation: {Rotation}, DestinationPosition: {DestinationPosition}, DestinationDirection: {DestinationDirection}, Velocity: {Velocity}]";
        }
    }
}
