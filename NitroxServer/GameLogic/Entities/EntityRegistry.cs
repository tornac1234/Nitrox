using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityRegistry
    {
        private readonly ConcurrentDictionary<NitroxId, Entity> entitiesById = new();

        public Optional<T> GetEntityById<T>(NitroxId id) where T : Entity
        {
            TryGetEntityById(id, out T entity);

            return Optional.OfNullable(entity);
        }

        public Optional<Entity> GetEntityById(NitroxId id)
        {
            return GetEntityById<Entity>(id);
        }

        public bool TryGetEntityById<T>(NitroxId id, out T entity) where T : Entity
        {
            if (entitiesById.TryGetValue(id, out Entity _entity) && _entity is T typedEntity)
            {
                entity = typedEntity;
                return true;
            }
            entity = null;
            return false;
        }

        public List<Entity> GetAllEntities(bool exceptGlobalRoot = false)
        {
            if (exceptGlobalRoot)
            {
                return new(entitiesById.Values.Where(entity => entity is not GlobalRootEntity));
            }
            return new List<Entity>(entitiesById.Values);            
        }

        public List<Entity> GetEntities(List<NitroxId> ids)
        {
            return entitiesById.Join(ids,
                                        entity => entity.Value.Id,
                                        id => id,
                                        (entity, id) => entity.Value)
                                .ToList();
        }

        public List<T> GetEntities<T>()
        {
            return entitiesById.Values.OfType<T>().ToList();
        }

        public void AddEntity(Entity entity)
        {
            if (!entitiesById.TryAdd(entity.Id, entity))
            {
                // Log an error to show stack trace but don't halt execution.
                Log.Error(new InvalidOperationException(), $"Trying to add duplicate entity {entity.Id}");
            }
        }

        public void AddOrUpdate(Entity entity)
        {
            if (!entitiesById.TryAdd(entity.Id, entity))
            {
                Entity current = entitiesById[entity.Id];

                RemoveFromParent(current);

                entitiesById.TryUpdate(entity.Id, entity, current);
            }

            AddToParent(entity);
            AddEntitiesIgnoringDuplicate(entity.ChildEntities);
        }

        public void AddEntities(IEnumerable<Entity> entities)
        {
            foreach(Entity entity in entities)
            {
                AddEntity(entity);
            }
        }

        /// <summary>
        /// Used for situations when some children may be new but others may not be. For
        /// example a dropped InventoryEntity turns into a WorldEntity but keeps its 
        /// battery inside (already known). 
        /// </summary>
        public void AddEntitiesIgnoringDuplicate(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                entitiesById.TryAdd(entity.Id, entity);
                AddEntitiesIgnoringDuplicate(entity.ChildEntities);
            }
        }

        public Optional<Entity> RemoveEntity(NitroxId id)
        {
            if (entitiesById.TryRemove(id, out Entity entity))
            {
                RemoveFromParent(entity);

                foreach (Entity child in entity.ChildEntities)
                {
                    RemoveEntity(child.Id);
                }
            }

            return Optional.OfNullable(entity);
        }

        public void AddToParent(Entity entity)
        {
            if (entity.ParentId != null)
            {
                Optional<Entity> parent = GetEntityById(entity.ParentId);

                if (parent.HasValue)
                {
                    parent.Value.ChildEntities.Add(entity);
                }
            }
        }

        public void RemoveFromParent(Entity entity)
        {
            if (entity.ParentId != null && TryGetEntityById(entity.ParentId, out Entity parentEntity))
            {
                // TODO: Either use this solution (to remove duplicated entities also) to avoid wrongly-referenced children
                // Or try fixing wrongly-referenced children (when entities are overwritten by AddOrUpdate)
                parentEntity.ChildEntities.RemoveAll(childEntity => childEntity.Id.Equals(entity.Id));
                entity.ParentId = null;
            }
        }

        public void ReparentEntity(NitroxId entityId, NitroxId newParentId)
        {
            Optional<Entity> opEntity = GetEntityById(entityId);

            if (!opEntity.HasValue)
            {
                Log.Error($"Could not find entity to reparent: {entityId}");
                return;
            }

            Entity entity = opEntity.Value;

            RemoveFromParent(entity);

            entity.ParentId = newParentId;

            AddToParent(entity);
        }
    }
}
