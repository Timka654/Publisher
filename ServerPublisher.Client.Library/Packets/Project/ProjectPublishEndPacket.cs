using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherPacketEnum.ProjectPublishEndResult)]
    internal class ProjectPublishEndPacket : IPacketReceive<NetworkClient, bool>
    {
        private static ProjectPublishEndPacket Instance { get; set; }

        public ProjectPublishEndPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) => Data = data.ReadBool();

        public static async Task<bool> Send(CommandLineArgs args)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PublisherPacketEnum.PublishProjectFinish);

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
