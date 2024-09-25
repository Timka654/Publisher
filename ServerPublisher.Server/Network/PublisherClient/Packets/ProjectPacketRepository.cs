using NSL.SocketCore.Utils.Buffer;
using System.Linq;
using ServerPublisher.Shared.Models.RequestModels;
using System.Threading.Tasks;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Info;

namespace ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository
{
    public class ProjectPacketRepository
    {
        public static async Task<bool> PublishProjectFileStartReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishFileStartRequestModel.ReadFullFrom(data);

            var context = client.PublishContext;

            var project = context?.ProjectInfo;

            response.WriteGuid(project.StartPublishFile(context, request));

            return true;
        }

        public static async Task<bool> PublishProjectFinishReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishProjectFinishRequestModel.ReadFullFrom(data);

            var context = client.PublishContext;

            var project = context?.ProjectInfo;

            project.FinishPublishProcess(context, true, request.Args);

            return true;
        }

        public static async Task<bool> PublishProjectSignInReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishSignInRequestModel.ReadFullFrom(data);

            SignStateEnum result = client.PublishContext != null ? SignStateEnum.Ok : default;

            if (result == default)
                result = PublisherServer.ProjectsManager.SignIn(client, request);

            new PublishSignInResponseModel
            {
                Result = result,
                IgnoreFilePatterns = client.PublishContext?.ProjectInfo.Info.IgnoreFilePaths
            }.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> PublishProjectUploadFilePartReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var context = client.PublishContext;

            if (context != null)
                return false;

            var request = PublishUploadFileBytesRequestModel.ReadFullFrom(data);

            return context.ProjectInfo.UploadPublishFile(context, request);
        }
    }
}
