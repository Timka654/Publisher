using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.PublisherClientPackets.ProjectPublishEndResult)]
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

            packet.SetPacketId(Basic.PublisherServerPackets.ProjectPublishEnd);

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
