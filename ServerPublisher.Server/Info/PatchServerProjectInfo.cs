using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using ServerPublisher.Server.Network.PublisherClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.Cipher.RSA;
using ServerPublisher.Shared.Enums;
using System.Threading.Tasks;
using NSL.Utils;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Models;
using System;

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

            var message = OutputPacketBuffer.Create(PublisherPacketEnum.ProjectProxyStartMessage);

            new ProjectProxyStartDownloadMessageModel()
            {
                ProjectId = Info.Id,
                FileList = FileInfoList.Select(x => new DownloadFileInfo()
                {
                    RelativePath = x.RelativePath,
                    Hash = x.Hash,
                    LastChanged = x.LastChanged,
                    CreationTime = x.FileInfo.CreationTimeUtc,
                    ModifiedTime = x.FileInfo.LastWriteTimeUtc
                }).ToArray()
            }.WriteDefaultTo(message);

            client.Send(message);
        }

        public ProjectProxyStartFileResponseModel StartDownloadFile(PublisherNetworkClient client, string relativePath)
        {
            var file = FileInfoList.FirstOrDefault(x => x.RelativePath == relativePath);

            if (file == null)
                return new ProjectProxyStartFileResponseModel() { Result = false };

            Guid fileId = default;

            while (!client.ProxyClientContext.TempFileMap.TryAdd(fileId = Guid.NewGuid(), file.OpenRead())) ;

            return new ProjectProxyStartFileResponseModel() { Result = true, FileId = fileId };
        }

        internal ProjectProxyEndDownloadResponseModel EndDownload(PublisherNetworkClient client, bool success = false)
        {
            patchLocker.Set();

            var response = new ProjectProxyEndDownloadResponseModel() { Success = success };

            if (success)
            {
                response.FileList = Directory.GetFiles(ScriptsDirPath, "*.cs")
                    .Select(d => new FileDownloadDataModel()
                    {
                        Data = File.ReadAllBytes(d),
                        RelativePath = Path.GetRelativePath(ProjectDirPath, d)
                    }).ToArray();
            }

            return response;
        }

        #endregion

    }
}
