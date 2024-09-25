using NSL.SocketCore.Utils.Buffer;
using System.Linq;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using System.Threading.Tasks;
using ServerPublisher.Shared.Info;
using ServerPublisher.Server.Info;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
{
    public class ProjectProxyPacketRepository
    {
        public static async Task<bool> ProjectProxyDownloadBytesReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxyDownloadBytesRequestModel.ReadFullFrom(data);

            var result = new ProjectProxyDownloadBytesResponseModel();

            if (client.ProxyClientContext.TempFileMap.TryGetValue(request.FileId, out var fs))
            {
                if (request.BufferLength > fs.Length - fs.Position)
                    request.BufferLength = (int)(fs.Length - fs.Position);

                result.Bytes = new byte[request.BufferLength];
                result.EOF = fs.Position + request.BufferLength == fs.Length;

                fs.Read(result.Bytes, 0, request.BufferLength);

                if (result.EOF)
                {
                    client.ProxyClientContext.TempFileMap.TryRemove(request.FileId, out _);
                    fs.Close();
                }
            }

            result.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxyStartFileReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxyStartFileRequestModel.ReadFullFrom(data);

            PublisherServer.ProjectProxyManager.StartFile(client, request).WriteFullTo(response);

            return true;
        }

        public static async Task<bool> ProjectProxyFinishDownloadReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = ProjectProxyEndDownloadRequestModel.ReadFullFrom(data);

            PublisherServer.ProjectProxyManager.FinishDownload(client, request).WriteFullTo(response);

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

            ServerProjectInfo project = null;

            if (client.ProxyClientContext?.PatchProjectMap.TryGetValue(request.ProjectId, out project) == true)
                project.StartDownload(client, Shared.Enums.TransportModeEnum.NoArchive);

            new Shared.Models.RequestModels.ProjectProxyStartDownloadResponseModel()
            {
                Result = project != null,
                IgnoreFilePathes = project?.Info.IgnoreFilePaths
            }.WriteFullTo(response);

            return true;
        }
    }
}
