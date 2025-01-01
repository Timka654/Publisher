using ServerPublisher.Server.Managers.Storages;
using ServerPublisher.Server.Network;
using ServerPublisher.Server.Info;
using System.Threading.Tasks;
using ServerPublisher.Server.Network.PublisherClient;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using System;

namespace ServerPublisher.Server.Managers
{
    internal class ProjectProxyManager : ProjectProxyStorage
    {
        static Lazy<ProjectProxyManager> instance = new(() => new ProjectProxyManager());

        public static ProjectProxyManager Instance => instance.Value;

        internal ProjectProxyEndDownloadResponseModel FinishDownload(PublisherNetworkClient client, ProjectProxyEndDownloadRequestModel request)
        {
            client.ProxyClientContext.PatchProjectMap.TryGetValue(request.ProjectId, out var project);

            return project.EndDownload(client, true);
        }

        internal ProjectProxyStartFileResponseModel? StartFile(PublisherNetworkClient client, ProjectProxyStartFileRequestModel request)
        {
            if (client.ProxyClientContext.PatchProjectMap.TryGetValue(request.ProjectId, out var project))
                return project.StartDownloadFile(client, request.RelativePath);

            return default;
        }

        public async Task<PatchClientNetwork> ConnectProxyClient(ServerProjectInfo project)
        {
            var client = GetClient(project.Info.PatchInfo.IpAddress, project.Info.PatchInfo.Port, () =>
            {

                var c = new PatchClientNetwork(project.Info.PatchInfo);

                return c;
            });

            client.SignProject(project);

            return client;
        }

        internal SignStateEnum SignIn(PublisherNetworkClient client, ProjectProxySignInRequestModel request)
        {
            var project = PublisherServer.ProjectsManager.GetProject(request.ProjectId);

            if (project == null)
                return SignStateEnum.ProjectNotFound;

            return project.SignPatchClient(client, request);
        }

        internal void SignOut(PublisherNetworkClient client, string projectId)
        {
            if (client.ProxyClientContext.PatchProjectMap.TryGetValue(projectId, out var project))
                project.SignOutPatchClient(client);
        }
    }
}
