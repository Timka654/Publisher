﻿using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Models.RequestModels;
using System.Threading.Tasks;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Enums;
using System;
using System.Linq;

namespace ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository
{
    public class ProjectPacketRepository
    {
        public static async Task<bool> PublishProjectFileStartReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishProjectFileStartRequestModel.ReadFullFrom(data);

            var context = client.PublishContext;

            var project = context?.ProjectInfo;

            var id = project?.StartPublishFile(context, request);

            new PublishProjectFileStartResponseModel()
            {
                Result = id.HasValue,
                FileId = id ?? Guid.Empty
            }.WriteFullTo(response);

            return true;
        }

        public static async Task<bool> PublishProjectFinishReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishProjectFinishRequestModel.ReadFullFrom(data);

            var context = client.PublishContext;

            var project = context?.ProjectInfo;

            response.WriteBool(await project?.FinishPublishProcess(context, true, request.Args) == true);

            return true;
        }

        public static async Task<bool> PublishProjectSignInReceive(PublisherNetworkClient client, InputPacketBuffer data, OutputPacketBuffer response)
        {
            var request = PublishSignInRequestModel.ReadFullFrom(data);

            SignStateEnum result = client.PublishContext != null ? SignStateEnum.Ok : default;

            if (result == default)
                result = await PublisherServer.ProjectsManager.SignIn(client, request);

            new PublishSignInResponseModel
            {
                Result = result,
                IgnoreFilePatterns = client.PublishContext?.ProjectInfo.IgnorePathsPatters.ToList()
            }.WriteFullTo(response);

            return true;
        }

        public static async Task PublishProjectUploadFilePartReceive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            var context = client.PublishContext;

            if (context == null)
                return;

            var request = PublishProjectUploadFileBytesRequestModel.ReadFullFrom(data);

            var p = OutputPacketBuffer.Create(PublisherPacketEnum.UploadPartIncrementMessage);
            p.WriteInt32(request.Bytes.Length);


            await context.ProjectInfo.UploadPublishFile(context, request);

            client.Send(p);
        }
    }
}
