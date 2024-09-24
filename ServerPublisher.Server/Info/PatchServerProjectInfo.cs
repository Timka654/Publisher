using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using NSL.Cipher.RSA;
using ServerPublisher.Shared.Enums;
using System.Threading.Tasks;
using NSL.Utils;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;

namespace ServerPublisher.Server.Info
{
    public partial class ServerProjectInfo
    {
        private AutoResetEvent patchLocker = new AutoResetEvent(true);

        #region Server

        private ConcurrentBag<PublisherNetworkClient> patchClients = new ConcurrentBag<PublisherNetworkClient>();

        private void broadcastUpdateTime(PublisherNetworkClient client = null)
        {
            using var packet = OutputPacketBuffer.Create(PublisherPacketEnum.ProjectProxyUpdateDataMessage);

            new ProjectProxyUpdateDataRequestModel()
            {
                ProjectId = Info.Id,
                UpdateTime = Info.LatestUpdate.Value
            }.WriteFullTo(packet);

            if (client == null)
                foreach (var item in patchClients)
                {
                    try { item.Send(packet, false); } catch { }
                }
            else
                client.Send(packet);
        }

        public SignStateEnum SignPatchClient(PublisherNetworkClient client, ProjectProxySignInRequestModel request)
        {
            client.ProxyClientContext ??= new ProxyClientContextDataModel();

            if (client.ProxyClientContext.PatchProjectMap.ContainsKey(Info.Id))
            {
                if (Info.LatestUpdate.HasValue && Info.LatestUpdate.Value > request.LatestUpdate)
                    broadcastUpdateTime(client);

                return SignStateEnum.Ok;
            }

            var user = users.FirstOrDefault(x => x.Id == request.UserId);

            if (user == null) return SignStateEnum.UserNotFound;

            if (user.Cipher == null)
            {
                user.Cipher = new RSACipher();

                user.Cipher.LoadXml(user.RSAPrivateKey);
            }

            byte[] data = user.Cipher.Decode(request.IdentityKey, 0, request.IdentityKey.Length);

            if (Encoding.ASCII.GetString(data) != request.UserId) return SignStateEnum.UserNotFound;

            patchClients.Add(client);

            client.ProxyClientContext.PatchProjectMap.TryAdd(Info.Id, this);


            if (Info.LatestUpdate.HasValue && Info.LatestUpdate.Value > request.LatestUpdate)
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    broadcastUpdateTime(client);
                }).RunAsync();

            return SignStateEnum.Ok;
        }

        public void SignOutPatchClient(PublisherNetworkClient client)
        {
            patchClients = new ConcurrentBag<PublisherNetworkClient>(patchClients.Where(x => x != client));

            EndDownload(client, true);
        }

        public void StartDownload(PublisherNetworkClient client, TransportModeEnum transportMode)
        {
            client.Lock(patchLocker);

            initializeLogger();
        }

        public bool NextDownloadFile(PublisherNetworkClient client, string relativePath)
        {
            client.CurrentFile = FileInfoList.FirstOrDefault(x => x.RelativePath == relativePath);

            if (client.CurrentFile != null && !client.CurrentFile.FileInfo.Exists)
            {
                FileInfoList.Remove(client.CurrentFile);
                client.CurrentFile = null;
            }

            if (client.CurrentFile != null)
                client.CurrentFile.OpenRead();

            return true;
        }

        internal ProjectProxyEndDownloadResponseModel EndDownload<T>(T client, bool success = false)
            where T : INetworkClient, IProcessFileContainer
        {
            if (client.CurrentFile != null)
            {
                client.CurrentFile.CloseRead();
                client.CurrentFile = null;
            }

            patchLocker.Set();

            if (success)
            {
                var packet = new OutputPacketBuffer();

                packet.SetPacketId(PatchClientPacketEnum.FinishDownloadResult);

                byte[] buf = null;

                packet.WriteCollection(Directory.GetFiles(ScriptsDirPath, "*.cs"), (p, d) =>
                {
                    buf = File.ReadAllBytes(d);
                    p.WritePath(Path.GetRelativePath(ProjectDirPath, d));
                    p.WriteInt32(buf.Length);
                    p.Write(buf);
                });

                client.Network?.Send(packet);
            }
        }

        #endregion

    }
}
