﻿using Publisher.Basic;
using SocketCore.Utils.Buffer;

namespace Publisher.Cliient.Network.Packets
{
    internal static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PublisherServerPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
