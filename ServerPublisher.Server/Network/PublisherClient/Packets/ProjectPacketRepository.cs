using NSL.SocketCore.Utils.Buffer;
using System.Linq;
using ServerPublisher.Shared.Models.RequestModels;
using System.Threading.Tasks;
using ServerPublisher.Shared.Models.ResponseModel;

namespace ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository
{
    public class ProjectPacketRepository
    {
        public static async Task<bool> PublishProjectFileStartReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            client.ProjectInfo.StartFile(client, PublishFileStartRequestModel.ReadFullFrom(data));

            return true;
        }

        public static async Task<bool> PublishProjectFileListReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var project = client.UserInfo.CurrentProject;

            if (client.UserInfo == null || project == null)
            {
                client.Network.Disconnect();
                return false;
            }

            if (project.ProcessUser != client.UserInfo)
            {
                if (!project.StartProcess(client))
                    return true;
            }

            new ProjectFileListResponseModel()
            {
                FileList = project.FileInfoList.Where(x => x.FileInfo.Exists).ToArray()
            }.WriteDefaultTo(response);

            return true;
        }

        public static async Task<bool> PublishProjectFinishReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishProjectFinishRequestModel.ReadFullFrom(data);

            client.ProjectInfo.StopProcess(client, true, request.Args);

            return true;
        }

        public static async Task<bool> PublishProjectSignInReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishSignInRequestModel.ReadFullFrom(data);

            response.WriteByte((byte)PublisherServer.ProjectsManager.SignIn(client, request));

            return true;
        }

        public static async Task<bool> PublishProjectUploadFilePartReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            client.CurrentFile.IO.Write(data.ReadByteArray());

            return true;
        }
    }
}
