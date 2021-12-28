﻿using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets;

[Serializable]
public class RocketLaunch : Packet
{
    public NitroxId RocketId;
    public RocketLaunch(NitroxId rocketId)
    {
        RocketId = rocketId;
    }
}
