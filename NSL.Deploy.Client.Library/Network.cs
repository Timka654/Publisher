using System.Threading.Tasks;
using System;
using NSL.TCP.Client;
using NSL.SocketClient;
using NSL.Cipher.RC.RC4;
using ServerPublisher.Shared.Enums;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Models.RequestModels;
using NSL.SocketCore.Utils.Buffer;

namespace ServerPublisher.Client.Library
{
    public partial class Network
    {
        TCPNetworkClient<NetworkClient, ClientOptions<NetworkClient>> Client { get; set; }

        public event Action<ProjectFileListResponseModel> OnPublishProjectStartMessage = (d) => { };

        public event Action<string> OnServerLogMessage = (d) => { };
        public event Action<int> OnUploadPartMessage = (d) => { };

        public Network(string ip, int port, string inputKey, string outputKey, Action<NetworkClient> disconnectedEvent)
        {
            Client = TCPClientEndPointBuilder.Create()
            .WithClientProcessor<NetworkClient>()
            .WithOptions()
            .WithEndPoint(ip, port)
            .WithCode(builder =>
            {
                builder.WithBufferSize(2048);

                builder.GetCoreOptions().SegmentSize = 64 * 1024;

                builder.AddConnectHandle(c => c.InitializeObjectBag());

                builder.GetOptions().ConfigureRequestProcessor();

                builder.AddPacketHandle(PublisherPacketEnum.PublishProjectStartMessage, (c, d) => OnPublishProjectStartMessage(ProjectFileListResponseModel.ReadDefaultFrom(d)));
                builder.AddPacketHandle(PublisherPacketEnum.ServerLog, (c, d) => OnServerLogMessage(d.ReadString()));
                builder.AddPacketHandle(PublisherPacketEnum.UploadPartIncrementMessage, (c, d) => OnUploadPartMessage(d.ReadInt32()));

                builder.WithInputCipher(new XRC4Cipher(inputKey));
                builder.WithOutputCipher(new XRC4Cipher(outputKey));

                builder.AddDisconnectHandle(c => disconnectedEvent?.Invoke(c));

                builder.AddExceptionHandle((ex, c) =>
                    Console.WriteLine(ex.ToString())
                );
            })
            .Build();
        }

        public Task<bool> ConnectAsync()
            => Client.ConnectAsync();

        public void Disconnect()
            => Client.Disconnect();
        private RequestProcessor? requestProcessor => Client.Data?.GetRequestProcessor();

        private async Task<TResult> Request<TResult>(PublisherPacketEnum pid
            , Action<RequestPacketBuffer> buildRequest
            , Func<InputPacketBuffer, TResult> readResponse)
        {
            var request = RequestPacketBuffer.Create(pid);

            buildRequest(request);

            TResult result = default;

            await requestProcessor.SendRequestAsync(request, data =>
            {
                if (data != null)
                    result = readResponse(data);

                return Task.FromResult(true);
            });

            return result;
        }

        private async Task Message(PublisherPacketEnum pid
            , Action<OutputPacketBuffer> buildRequest)
        {
            var request = OutputPacketBuffer.Create(pid);

            buildRequest(request);

            Client.Send(request);
        }

        public async Task<PublishSignInResponseModel> SignIn(PublishSignInRequestModel request)
            => await Request(PublisherPacketEnum.PublishProjectSignIn, request.WriteFullTo, PublishSignInResponseModel.ReadFullFrom);

        public async Task<bool> ProjectFinish(PublishProjectFinishRequestModel request)
            => await Request(PublisherPacketEnum.PublishProjectFinish, request.WriteFullTo, r => r?.ReadBool() ?? false);

        public async Task<PublishProjectFileStartResponseModel> FileStart(PublishProjectFileStartRequestModel request)
            => await Request(PublisherPacketEnum.PublishProjectFileStart, request.WriteFullTo, PublishProjectFileStartResponseModel.ReadFullFrom);

        public async Task<bool> UploadFilePart(PublishProjectUploadFileBytesRequestModel request)
        {
            await Message(PublisherPacketEnum.PublishProjectUploadFilePart, req=> request.WriteFullTo(req));
            
            return true;
        } 
    }
}
