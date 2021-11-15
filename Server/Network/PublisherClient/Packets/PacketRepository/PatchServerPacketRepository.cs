using Publisher.Basic;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.PublisherClient.Packets
{
    [ServerPacketDelegateContainer]
    public class PatchServerPacketRepository
    {
        #region DownloadBytes

        [ServerPacket(PatchServerPackets.DownloadBytes)]
        public static void DownloadBytesReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null || client.CurrentFile == null)
            {
                client.Network?.Disconnect();
                return;
            }

            SendDownloadBytes(client, data.ReadInt32());
        }

        private static void SendDownloadBytes(PublisherNetworkClient client, int bufLen)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchClientPackets.DownloadBytesResult);

            byte[] result = new byte[bufLen];

            int len = client.CurrentFile.IO.Read(result, 0, bufLen);

            packet.WriteInt32(len);

            packet.Write(result, 0, len);

            packet.WriteBool(client.CurrentFile.IO.Position == client.CurrentFile.IO.Length);

            client.Send(packet);
        }

        #endregion

        #region FinishDownload

        [ServerPacket(PatchServerPackets.FinishDownload)]
        public static void FinishDownloadReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            StaticInstances.PatchManager.FinishDownload(client);
        }

        #endregion

        #region NextFile

        [ServerPacket(PatchServerPackets.NextFile)]
        public static void NextFileReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return;
            }

            client.PatchDownloadProject.NextDownloadFile(client, data.ReadPath());
        }

        #endregion

        #region ProjectFileList

        [ServerPacket(PatchServerPackets.ProjectFileList)]
        public static void ProjectFileListReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return;
            }

            SendProjectFileList(client);
        }

        private static void SendProjectFileList(PublisherNetworkClient client)
        {
            var project = client.PatchDownloadProject;

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchClientPackets.ProjectFileListResult);

            packet.WriteCollection(project.FileInfoList.Where(x => x.FileInfo.Exists), (p, item) =>
            {
                p.WritePath(item.RelativePath);
                p.WriteString16(item.Hash);
                p.WriteDateTime(item.LastChanged);
                p.WriteDateTime(item.FileInfo.CreationTimeUtc);
                p.WriteDateTime(item.FileInfo.LastWriteTimeUtc);
            });

            client.Send(packet);
        }

        #endregion

        #region SignIn

        [ServerPacket(PatchServerPackets.SignIn)]
        public static void SignInReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            DateTime latestUpdate = data.ReadDateTime();

            StaticInstances.PatchManager.SignIn(client, projectId, userId, key, latestUpdate);
        }

        public static void SendSignInResult(PublisherNetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PatchClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }

        #endregion

        #region SignOut

        [ServerPacket(PatchServerPackets.SignOut)]
        public static void SignOutReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string projectId = data.ReadString16();

            StaticInstances.PatchManager.SignOut(client, projectId);
        }

        #endregion

        #region StartDownload

        [ServerPacket(PatchServerPackets.StartDownload)]
        public static void StartDownloadReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string projectId = data.ReadString16();


            TransportModeEnum transportMode = TransportModeEnum.NoArchive;

            if (data.Offset != data.Lenght)
            {
                transportMode = (TransportModeEnum)data.ReadByte();
            }

            StaticInstances.PatchManager.StartDownload(client, projectId, transportMode);
        }

        public static void SendStartDownloadResult(PublisherNetworkClient client, bool result, List<string> ignoreFilePaths)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PatchClientPackets.StartDownloadResult);
            packet.WriteBool(result);
            packet.WriteCollection(ignoreFilePaths, (b, v) => b.WritePath(v));

            client.Send(packet);
        }

        #endregion
    }
}
