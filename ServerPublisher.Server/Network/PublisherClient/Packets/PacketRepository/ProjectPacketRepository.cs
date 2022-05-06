using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository
{
    [ServerPacketDelegateContainer]
    public class ProjectPacketRepository
    {
        #region FilePublishStart

        [ServerPacket(PublisherServerPackets.FilePublishStart)]
        public static void FilePublishStartReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            client.ProjectInfo.StartFile(client, data.ReadPath(), data.ReadDateTime(), data.ReadDateTime());

            client.Network.SendEmpty((byte)PublisherClientPackets.FilePublishStartResult);
        }

        #endregion

        #region ProjectFileList

        [ServerPacket(PublisherServerPackets.ProjectFileList)]
        public static void ProjectFileListReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.UserInfo == null || client.UserInfo.CurrentProject == null)
            {
                client.Network.Disconnect();
                return;
            }

            if (client.UserInfo.CurrentProject.ProcessUser != client.UserInfo)
            {
                if (!client.UserInfo.CurrentProject.StartProcess(client))
                    return;
            }

            SendFileListResult(client);
        }

        private static void SendFileListResult(PublisherNetworkClient client)
        {
            var project = client.UserInfo.CurrentProject;

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PublisherClientPackets.FileListResult);

            packet.WriteCollection(project.FileInfoList.Where(x => x.FileInfo.Exists), (p, item) =>
            {
                p.WritePath(item.RelativePath);
                p.WriteString16(item.Hash);
                p.WriteDateTime(item.LastChanged);
            });

            client.Send(packet);
        }

        #endregion

        #region ProjectPublishEnd

        [ServerPacket(PublisherServerPackets.ProjectPublishEnd)]
        public static void ProjectPublishEndReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            int c = data.ReadByte();

            for (int i = 0; i < c; i++)
            {
                args.Add(data.ReadString16(), data.ReadString16());
            }

            client.ProjectInfo.StopProcess(client, true, args);

            SendProjectPublishEndResult(client);
        }

        public static void SendProjectPublishEndResult(PublisherNetworkClient client)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherClientPackets.ProjectPublishEndResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }

        #endregion

        #region SignIn

        [ServerPacket(PublisherServerPackets.SignIn)]
        public static void SignInReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            client.Platform = (OSTypeEnum)data.ReadByte();

            var compressed = data.ReadBool();

            StaticInstances.ProjectsManager.SignIn(client, projectId, userId, key, compressed);
        }

        public static void SendSignInResult(PublisherNetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }

        #endregion

        #region UploadFileBytes

        [ServerPacket(PublisherServerPackets.UploadFileBytes)]
        public static void UploadFileBytesReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            client.CurrentFile.IO.Write(data.Read(data.ReadInt32()));

            SendUploadFileBytesResult(client);
        }

        public static void SendUploadFileBytesResult(PublisherNetworkClient client)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherClientPackets.UploadFileBytesResult);
            packet.WriteBool(true);

            client.Send(packet);
        }

        #endregion
    }
}
