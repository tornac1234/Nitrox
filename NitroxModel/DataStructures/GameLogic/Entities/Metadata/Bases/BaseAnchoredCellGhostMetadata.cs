using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;

[Serializable, DataContract]
public class BaseAnchoredCellGhostMetadata : GhostMetadata
{
    [DataMember(Order = 1)]
    public NitroxInt3? AnchoredCell { get; set; }

    public BaseAnchoredCellGhostMetadata()
    {
        // Constructor for ProtoBuf deserialization.
    }

    /// <remarks>Used for json deserialization</remarks>
    public BaseAnchoredCellGhostMetadata(NitroxInt3? anchoredCell, NitroxInt3 targetOffset) : base(targetOffset)
    {
        AnchoredCell = anchoredCell;
    }
}
