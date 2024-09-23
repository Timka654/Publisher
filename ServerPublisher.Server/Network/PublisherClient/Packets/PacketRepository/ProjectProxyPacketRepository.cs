using NSL.SocketCore.Utils.Buffer;
using System.Linq;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using System.Threading.Tasks;
using ServerPublisher.Shared.Info;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
{
    public class ProjectProxyPacketRepository
    {
        public static async Task<bool> ProjectProxyDownloadBytesReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            if (client.PatchDownloadProject == null || client.CurrentFile == null)
            {
                client.Network?.Disconnect();
                return false;
            }

            var request = ProjectProxyDownloadBytesRequestModel.ReadFullFrom(data);

            if (request.BufferLength > client.CurrentFile.IO.Length - client.CurrentFile.IO.Position)
                request.BufferLength = (int)(client.CurrentFile.IO.Length - client.CurrentFile.IO.Position);

            var result = new ProjectProxyDownloadBytesResponseModel()
            {
                Bytes = new byte[request.BufferLength],
                EOF = client.CurrentFile.IO.Position + request.BufferLength == client.CurrentFile.IO.Length
            };

            client.CurrentFile.IO.Read(result.Bytes, 0, request.BufferLength);

            result.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxyFinishDownloadReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            PublisherServer.ProjectProxyManager.FinishDownload(client);

            return true;
        }

        public static async Task<bool> ProjectProxyNextFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return false;
            }

            var request = ProjectProxyNextFileRequestModel.ReadFullFrom(data);

            new ProjectProxyNextFileResponseModel()
            {
                Result = client.PatchDownloadProject.NextDownloadFile(client, request.Path)
            }.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxyProjectFileListReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return false;
            }

            new ProjectProxyProjectFileListResponseModel()
            {
                FileList = client.PatchDownloadProject.FileInfoList.Where(x => x.FileInfo.Exists).Select(x => new DownloadFileInfo()
                {
                    RelativePath = x.RelativePath,
                    Hash = x.Hash,
                    LastChanged = x.LastChanged,
                    CreationTime = x.FileInfo.CreationTimeUtc,
                    ModifiedTime = x.FileInfo.LastWriteTimeUtc
                }).ToArray()
            }.WriteDefaultTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxySignInReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxySignInRequestModel.ReadFullFrom(data);

            new ProjectProxySignInResponseModel
            {
                Result = PublisherServer.ProjectProxyManager.SignIn(client, request)
            }.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxySignOutReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxySignOutResponseModel.ReadFullFrom(data);

            PublisherServer.ProjectProxyManager.SignOut(client, request.ProjectId);

            return true;
        }

        public static async Task<bool> ProjectProxyStartDownloadReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxyStartDownloadRequestModel.ReadFullFrom(data);

            var project = PublisherServer.ProjectProxyManager.StartDownload(client, request);

            new ProjectProxyStartDownloadResponseModel()
            {
                Result = project != null,
                IgnoreFilePathes = project?.Info.IgnoreFilePaths
            }.WriteFullTo(response);

            return true;
        }
    }
}
