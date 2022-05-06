using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Utils;
using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
{
    [ServerPacketDelegateContainer]
    public class ExplorerPacketRepository
    {
        #region ExplorerCreateSignFile

        [ServerPacket(PublisherServerPackets.ExplorerCreateSignFile)]
        public static void ExplorerCreateSignFileReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {

        }

        public static void SendExplorerCreateSignFileResult(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerCreateSignFileResult, (byte)result);

        #endregion

        #region ExplorerDownloadFile

        [ServerPacket(PublisherServerPackets.ExplorerDownloadFile)]
        public static void ExplorerDownloadFileReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            var projectId = data.ReadNullableClass(data.ReadString16);
            var filePath = data.ReadString16();

            SendExplorerDownloadFile(client, ExplorerManager.Instance.DownloadFile(projectId, filePath));
        }

        private static void SendExplorerDownloadFile(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerDownloadFileResult, (byte)result);

        #endregion

        #region ExplorerGetFileList

        [ServerPacket(PublisherServerPackets.ExplorerGetFileList)]
        public static void ExplorerGetFileListReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerGetFileList(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerGetFileListResult, (byte)result);

        #endregion

        #region ExplorerGetProjectList

        [ServerPacket(PublisherServerPackets.ExplorerGetProjectList)]
        public static void ExplorerGetProjectListReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerGetProjectList(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerGetProjectListResult, (byte)result);

        #endregion

        #region ExplorerPathRemove

        [ServerPacket(PublisherServerPackets.ExplorerPathRemove)]
        public static void ExplorerPathRemoveReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerPathRemove(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerPathRemoveResult, (byte)result);

        #endregion

        #region ExplorerRemoveSignFile

        [ServerPacket(PublisherServerPackets.ExplorerRemoveSignFile)]
        public static void ExplorerRemoveSignFileReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerRemoveSignFile(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerRemoveSignFileResult, (byte)result);

        #endregion

        #region ExplorerSignIn

        [ServerPacket(PublisherServerPackets.ExplorerSignIn)]
        public static void ExplorerSignInReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerSignIn(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerSignInResult, (byte)result);

        #endregion

        #region ExplorerUploadFile

        [ServerPacket(PublisherServerPackets.ExplorerUploadFile)]
        public static void ExplorerUploadFileReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void SendExplorerUploadFile(PublisherNetworkClient client, ExplorerActionResultEnum result) => client.Network.Send(PublisherClientPackets.ExplorerUploadFileResult, (byte)result);

        #endregion
    }
}
