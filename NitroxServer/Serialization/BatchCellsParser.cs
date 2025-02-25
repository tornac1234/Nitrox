using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;
using ProtoBufNet;

namespace NitroxServer.Serialization
{
    /**
     * Parses the files in build18 in the format of batch-cells-x-y-z-slot-type.bin
     * These files contain serialized GameObjects with EntitySlot components. These
     * represent areas that entities (creatures, objects) can spawn within the world.
     * This class consolidates the gameObject, entitySlot, and cellHeader data to
     * create EntitySpawnPoint objects.
     */
    public class BatchCellsParser
    {
        private readonly EntitySpawnPointFactory entitySpawnPointFactory;
        private readonly ServerProtoBufSerializer serializer;
        private readonly Dictionary<string, Type> surrogateTypes;

        public BatchCellsParser(EntitySpawnPointFactory entitySpawnPointFactory, ServerProtoBufSerializer serializer)
        {
            this.entitySpawnPointFactory = entitySpawnPointFactory;
            this.serializer = serializer;

            surrogateTypes = new Dictionary<string, Type>
            {
                { "UnityEngine.Transform", typeof(NitroxTransform) },
                { "UnityEngine.Vector3", typeof(NitroxVector3) },
                { "UnityEngine.Quaternion", typeof(NitroxQuaternion) }
            };
        }

        public List<EntitySpawnPoint> ParseBatchData(NitroxInt3 batchId)
        {
            List<EntitySpawnPoint> spawnPoints = new List<EntitySpawnPoint>();

            ParseFile(batchId, "CellsCache", "baked-", "", spawnPoints);

            return spawnPoints;
        }

        public void ParseFile(NitroxInt3 batchId, string pathPrefix, string prefix, string suffix, List<EntitySpawnPoint> spawnPoints)
        {
            string subnauticaPath = NitroxUser.GamePath;
            if (string.IsNullOrEmpty(subnauticaPath))
            {
                return;
            }

            string path = Path.Combine(subnauticaPath, GameInfo.Subnautica.DataFolder, "StreamingAssets", "SNUnmanagedData", "Build18");
            string fileName = Path.Combine(path, pathPrefix, $"{prefix}batch-cells-{batchId.X}-{batchId.Y}-{batchId.Z}{suffix}.bin");

            if (!File.Exists(fileName))
            {
                return;
            }

            ParseCacheCells(batchId, fileName, spawnPoints);
        }

        /**
         * It is suspected that 'cache' is a misnomer carried over from when UWE was actually doing procedurally
         * generated worlds.  In the final release, this 'cache' has simply been baked into a final version that
         * we can parse.
         */
        private void ParseCacheCells(NitroxInt3 batchId, string fileName, List<EntitySpawnPoint> spawnPoints)
        {
            using Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            CellsFileHeader cellsFileHeader = serializer.Deserialize<CellsFileHeader>(stream);

            for (int cellCounter = 0; cellCounter < cellsFileHeader.NumCells; cellCounter++)
            {
                CellHeaderEx cellHeader = serializer.Deserialize<CellHeaderEx>(stream);

                byte[] serialData = new byte[cellHeader.DataLength];
                stream.ReadStreamExactly(serialData, serialData.Length);
                ParseGameObjectsWithHeader(serialData, batchId, cellHeader.CellId, cellHeader.Level, spawnPoints, out bool wasLegacy);

                if (!wasLegacy)
                {
                    byte[] legacyData = new byte[cellHeader.LegacyDataLength];
                    stream.ReadStreamExactly(legacyData, legacyData.Length);
                    ParseGameObjectsWithHeader(legacyData, batchId, cellHeader.CellId, cellHeader.Level, spawnPoints, out _);

                    byte[] waiterData = new byte[cellHeader.WaiterDataLength];
                    stream.ReadStreamExactly(waiterData, waiterData.Length);
                    ParseGameObjectsFromStream(new MemoryStream(waiterData), batchId, cellHeader.CellId, cellHeader.Level, spawnPoints);
                }
            }
        }

        private void ParseGameObjectsWithHeader(byte[] data, NitroxInt3 batchId, NitroxInt3 cellId, int level, List<EntitySpawnPoint> spawnPoints, out bool wasLegacy)
        {
            wasLegacy = false;

            if (data.Length == 0)
            {
                return;
            }

            using Stream stream = new MemoryStream(data);
            StreamHeader header = serializer.Deserialize<StreamHeader>(stream);

            if (ReferenceEquals(header, null))
            {
                return;
            }

            ParseGameObjectsFromStream(stream, batchId, cellId, level, spawnPoints);

            wasLegacy = header.Version < 9;
        }

        private void ParseGameObjectsFromStream(Stream stream, NitroxInt3 batchId, NitroxInt3 cellId, int level, List<EntitySpawnPoint> spawnPoints)
        {
            LoopHeader gameObjectCount = serializer.Deserialize<LoopHeader>(stream);

            for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
            {
                GameObject gameObject = DeserializeGameObject(stream);
                DeserializeComponents(stream, gameObject);

                // If it is an "Empty" GameObject, we need it to have serialized components
                if (!gameObject.CreateEmptyObject || gameObject.SerializedComponents.Count > 0)
                {
                    AbsoluteEntityCell absoluteEntityCell = new AbsoluteEntityCell(batchId, cellId, level);
                    NitroxTransform transform = gameObject.GetComponent<NitroxTransform>();
                    spawnPoints.AddRange(entitySpawnPointFactory.From(absoluteEntityCell, transform, gameObject));
                }
            }
        }

        private GameObject DeserializeGameObject(Stream stream)
        {
            return new(serializer.Deserialize<GameObjectData>(stream));
        }

        private void DeserializeComponents(Stream stream, GameObject gameObject)
        {
            gameObject.SerializedComponents.Clear();
            LoopHeader components = serializer.Deserialize<LoopHeader>(stream);

            for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
            {
                ComponentHeader componentHeader = serializer.Deserialize<ComponentHeader>(stream);

                if (!surrogateTypes.TryGetValue(componentHeader.TypeName, out Type type))
                {
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType(componentHeader.TypeName))
                        .FirstOrDefault(t => t != null);
                }

                Validate.NotNull(type, $"No type or surrogate found for {componentHeader.TypeName}!");

#if NET5_0_OR_GREATER
                object component = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
#else
                object component = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#endif

                long startPosition = stream.Position;
                serializer.Deserialize(stream, component, type);

                gameObject.AddComponent(component, type);
                // SerializedComponents only matter if this is an "Empty" GameObject
                if (gameObject.CreateEmptyObject && !type.Name.Equals(nameof(NitroxTransform)) && !type.Name.Equals("LargeWorldEntity"))
                {
                    byte[] data = new byte[(int)(stream.Position - startPosition)];
                    stream.Position = startPosition;
                    stream.ReadStreamExactly(data, data.Length);
                    SerializedComponent serializedComponent = new(componentHeader.TypeName, componentHeader.IsEnabled, data);
                    gameObject.SerializedComponents.Add(serializedComponent);
                }

            }
        }
    }

    [ProtoContract]
    public class CellsFileHeader
    {
        public override string ToString()
        {
            return string.Format("(version={0}, numCells={1})", Version, NumCells);
        }

        [ProtoMember(1)]
        public int Version;

        [ProtoMember(2)]
        public int NumCells;
    }

    [ProtoContract]
    public class CellHeader
    {
        public override string ToString()
        {
            return $"(cellId={CellId}, level={Level})";
        }

        [ProtoMember(1)]
        public NitroxInt3 CellId;

        [ProtoMember(2)]
        public int Level;
    }

    [ProtoContract]
    public class CellHeaderEx
    {
        public override string ToString()
        {
            return string.Format("(cellId={0}, level={1}, dataLength={2}, legacyDataLength={3}, waiterDataLength={4})", new object[]
            {
                CellId,
                Level,
                DataLength,
                LegacyDataLength,
                WaiterDataLength
            });
        }

        [ProtoMember(1)]
        public NitroxInt3 CellId;

        [ProtoMember(2)]
        public int Level;

        [ProtoMember(3)]
        public int DataLength;

        [ProtoMember(4)]
        public int LegacyDataLength;

        [ProtoMember(5)]
        public int WaiterDataLength;

        // There's no point in spawning allowSpawnRestrictions as SpawnRestrictionEnforcer doesn't load any restrictions
    }

    [ProtoContract]
    public class StreamHeader
    {
        [ProtoMember(1)]
        public int Signature
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public int Version
        {
            get;
            set;
        }

        public void Reset()
        {
            Signature = 0;
            Version = 0;
        }

        public override string ToString()
        {
            return string.Format("(UniqueIdentifier={0}, Version={1})", Signature, Version);
        }
    }

    [ProtoContract]
    public class LoopHeader
    {
        [ProtoMember(1)]
        public int Count
        {
            get;
            set;
        }

        public void Reset()
        {
            Count = 0;
        }

        public override string ToString()
        {
            return string.Format("(Count={0})", Count);
        }
    }

    [ProtoContract]
    public class GameObjectData
    {
        [ProtoMember(1)]
        public bool CreateEmptyObject
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public bool IsActive
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public int Layer
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public string Tag
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public string Id
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public string ClassId
        {
            get;
            set;
        }

        [ProtoMember(8)]
        public string Parent
        {
            get;
            set;
        }

        [ProtoMember(9)]
        public bool OverridePrefab
        {
            get;
            set;
        }

        [ProtoMember(10)]
        public bool MergeObject
        {
            get;
            set;
        }

        public void Reset()
        {
            CreateEmptyObject = false;
            IsActive = false;
            Layer = 0;
            Tag = null;
            Id = null;
            ClassId = null;
            Parent = null;
            OverridePrefab = false;
            MergeObject = false;
        }

        public override string ToString()
        {
            return string.Format("(CreateEmptyObject={0}, IsActive={1}, Layer={2}, Tag={3}, Id={4}, ClassId={5}, Parent={6}, OverridePrefab={7}, MergeObject={8})", new object[]
            {
                CreateEmptyObject,
                IsActive,
                Layer,
                Tag,
                Id,
                ClassId,
                Parent,
                OverridePrefab,
                MergeObject
            });
        }
    }

    [ProtoContract]
    public class ComponentHeader
    {
        [ProtoMember(1)]
        public string TypeName
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public bool IsEnabled
        {
            get;
            set;
        }

        public void Reset()
        {
            TypeName = null;
            IsEnabled = false;
        }

        public override string ToString()
        {
            return string.Format("(TypeName={0}, IsEnabled={1})", TypeName, IsEnabled);
        }
    }

}
