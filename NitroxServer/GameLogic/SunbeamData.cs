using System;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic;

[Serializable]
[ProtoContract, JsonObject(MemberSerialization.OptIn)]
public class SunbeamData
{
    [JsonProperty, ProtoMember(1)]
    public bool CountdownActive { get; set; }

    [JsonProperty, ProtoMember(2)]
    public double CountdownStartingTimeMs { get; set; }

    public InitialSunbeamData GetInitialSunbeamData()
    {
        return new InitialSunbeamData(CountdownActive, CountdownStartingTimeMs);
    }
}
