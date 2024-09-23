using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
{
    public class ExplorerPacketRepository
    {
        public static async Task<bool> ExplorerCreateSignFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerDownloadFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerGetFileListReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerGetProjectListReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerPathRemoveReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerRemoveSignFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerSignInReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }

        public static async Task<bool> ExplorerUploadFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {

            return true;
        }
    }
}
