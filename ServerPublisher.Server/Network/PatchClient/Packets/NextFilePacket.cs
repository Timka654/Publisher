﻿//using ServerPublisher.Shared;
//using NSL.SocketCore.Utils.Buffer;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    public class NextFilePacket
//    {
//        public static void Send(NetworkProjectProxyClient client, string relativePath)
//        {
//            var packet = new OutputPacketBuffer();

//            packet.SetPacketId(PatchServerPacketEnum.NextFile);

//            packet.WritePath(relativePath);

//            client.Network.Send(packet);
//        }
//    }
//}
