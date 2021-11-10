﻿using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.PublisherServerPackets.ProjectPublishEnd)]
    public class ProjectProcessEndPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            int c = data.ReadByte();

            for (int i = 0; i < c; i++)
            {
                args.Add(data.ReadString16(), data.ReadString16());
            }

            client.ProjectInfo.StopProcess(client, true, args);


            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.PublisherClientPackets.ProjectPublishEndResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }
    }
}