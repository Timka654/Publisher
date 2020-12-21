using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.SignInResult)]
    internal class SignInPacket : IPacketReceive<NetworkClient,SignStateEnum>
    {
        private static SignInPacket Instance;
        public SignInPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = (SignStateEnum)data.ReadByte();
        }

        public static async Task<SignStateEnum> Send(string projectId, BasicUserInfo user, byte[] encoded)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(ServerPackets.SignIn);
            packet.WriteString16(user.Id);
            packet.WriteString16(projectId);
            packet.WriteInt32(encoded.Length);
            packet.Write(encoded);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                packet.WriteByte((byte)OSTypeEnum.Windows);
            else
                packet.WriteByte((byte)OSTypeEnum.Unix);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
