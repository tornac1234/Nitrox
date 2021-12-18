﻿using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StoryEventSend : Packet
    {
        public EventType Type { get; }
        public string Key { get; }

        public StoryEventSend(EventType type, string key = "")
        {
            Type = type;
            Key = key;
        }

        // The 4 first items of this enum should always be in the same order as the GoalType enum
        public enum EventType
        {
            PDA,
            RADIO,
            ENCYCLOPEDIA,
            STORY,
            GOAL_UNLOCK,
            EXTRA,
            PDA_EXTRA,
        }
    }
}
