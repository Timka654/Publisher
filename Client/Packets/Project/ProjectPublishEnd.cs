﻿using Publisher.Cliient.Network.Packets;
using Publisher.Server.Tools;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.ProjectPublishEndResult)]
    public class ProjectPublishEnd : IPacketReceive<NetworkClient, bool>
    {
        private static ProjectPublishEnd Instance { get; set; }

        public ProjectPublishEnd(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = data.ReadBool();
        }

        public static async Task<bool> Send(CommandLineArgs args)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.ServerPackets.ProjectEnd);

            var a = args.GetArgs();

            packet.WriteByte((byte)a.Length);

            foreach (var item in a)
            {
                packet.WriteString16(item.Key);
                packet.WriteString16(item.Value);
            }


            return await Instance.SendWaitAsync(packet);

        }
    }
}
