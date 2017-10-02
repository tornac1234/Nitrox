﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using System;

namespace NitroxClient.Communication.Packets.Processors
{
    class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
        private readonly PacketSender packetSender;

        public ToggleLightsProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(NitroxModel.Packets.ToggleLights packet)
        {
            var opGameObject = GuidHelper.GetObjectFrom(packet.Guid);
            if (opGameObject.IsPresent())
            {
                var gameObject = opGameObject.Get();
                var toggleLights = gameObject.GetComponent<ToggleLights>();
                if (!toggleLights)
                {
                    toggleLights = gameObject.GetComponentInChildren<ToggleLights>();
                }
                if (toggleLights)
                {
                    if (packet.IsOn != toggleLights.GetLightsActive())
                    {
                        using (packetSender.Suppress<NitroxModel.Packets.ToggleLights>())
                        {
                            toggleLights.SetLightsActive(packet.IsOn);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Cannot find ToggleLights in gameObject or children of gameObject " + gameObject);
                }
            }
            else
            {
                Console.WriteLine($"ToggleLightsProcessor: Cannot find gameObject with guid {packet.Guid}");
            }
        }
    }
}