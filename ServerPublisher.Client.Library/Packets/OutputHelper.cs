﻿using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;

namespace ServerPublisher.Client.Library.Packets
{
    internal static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PublisherServerPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}