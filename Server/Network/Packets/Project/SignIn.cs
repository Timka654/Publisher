using Publisher.Basic;
using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.SignIn)]
    public class SignIn : IPacket<NetworkClient>
    {
        public void Receive(NetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            StaticInstances.ProjectsManager.SignIn(client,projectId, userId, key);
        }

        public static void Send(NetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(ClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }
    }
}
