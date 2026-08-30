using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class EntityTransformUpdatesProcessor(SimulationOwnership simulationOwnership) : IClientPacketProcessor<EntityTransformUpdates>
{
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;

    public Task Process(ClientProcessorContext context, EntityTransformUpdates packet)
    {
        foreach (EntityTransformUpdate update in packet.Updates)
        {
            // We will cancel any position update attempt at one of our locked entities
            if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject) ||
                simulationOwnership.HasAnyLockType(update.Id))
            {
                continue;
            }

            RemotelyControlled remotelyControlled = RemotelyControlled.Ensure(gameObject);

            Vector3 position = update.Position.ToUnity();
            Quaternion rotation = update.Rotation.ToUnity();

            switch (update)
            {
                case LocomotionUpdate locomotionUpdate:
                    remotelyControlled.UpdateLocomotion(position, rotation, locomotionUpdate.DestinationPosition.ToUnity(), locomotionUpdate.DestinationDirection.ToUnity(), locomotionUpdate.Velocity);
                    break;
                default:
                    remotelyControlled.UpdateOrientation(position, rotation);
                    break;
            }
        }
        return Task.CompletedTask;
    }
}
