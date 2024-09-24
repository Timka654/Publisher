﻿//using NSL.SocketClient;
//using NSL.SocketClient.Utils;
//using NSL.SocketCore.Utils.Buffer;
//using System;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using ServerPublisher.Shared.Enums;
//using ServerPublisher.Server.Network.PublisherClient.Packets;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    [ServerPacket(PublisherPacketEnum.SignInResult)]
//    internal class SignInPacket : IPacketReceive<NetworkProjectProxyClient, SignStateEnum>
//    {
//        protected override void Receive(InputPacketBuffer data) => Data = (SignStateEnum)data.ReadByte();

//        public async Task<SignStateEnum> Send(string projectId, string userId, byte[] encoded, DateTime? latestUpdate)
//        {
//            var packet = new OutputPacketBuffer();
//            packet.SetPacketId(PublisherPacketEnum.PublishSignIn);
//            packet.WriteString16(userId);
//            packet.WriteString16(projectId);
//            packet.WriteInt32(encoded.Length);
//            packet.Write(encoded);

//            packet.WriteDateTime(latestUpdate ?? DateTime.MinValue);

//            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//                packet.WriteByte((byte)OSTypeEnum.Windows);
//            else
//                packet.WriteByte((byte)OSTypeEnum.Unix);

//            return await SendWaitAsync(packet);
//        }

//        public SignInPacket(ClientOptions<NetworkProjectProxyClient> options) : base(options) { }
//    }
//}
