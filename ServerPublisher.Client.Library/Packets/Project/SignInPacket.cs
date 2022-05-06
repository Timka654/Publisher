using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherClientPackets.SignInResult)]
    internal class SignInPacket : IPacketReceive<NetworkClient,SignStateEnum>
    {
        private static SignInPacket Instance;
        public SignInPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) => Data = (SignStateEnum)data.ReadByte();

        public static async Task<SignStateEnum> Send(string projectId, BasicUserInfo user, byte[] encoded, bool compressed)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherServerPackets.SignIn);

            packet.WriteString16(user.Id);

            packet.WriteString16(projectId);

            packet.WriteInt32(encoded.Length);

            packet.Write(encoded);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                packet.WriteByte((byte)OSTypeEnum.Windows);
            else
                packet.WriteByte((byte)OSTypeEnum.Unix);

            packet.WriteBool(compressed);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
