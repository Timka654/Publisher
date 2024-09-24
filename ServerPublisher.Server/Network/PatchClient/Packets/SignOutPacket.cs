//using NSL.SocketCore.Utils.Buffer;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    internal class SignOutPacket
//    {
//        public static void Send(NetworkProjectProxyClient client, string projectId)
//        {
//            var packet = new OutputPacketBuffer();

//            packet.SetPacketId(PatchServerPacketEnum.SignOut);

//            packet.WriteString16(projectId);

//            client.Send(packet);
//        }
//    }
//}
