using ServerPublisher.Server.Managers.Storages;
using ServerPublisher.Server.Network;
using ServerPublisher.Server.Info;
using System.Threading.Tasks;
using ServerPublisher.Server.Network.PublisherClient;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Models.RequestModels;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(0)]
    internal class ProjectProxyManager : ProjectProxyStorage
    {
        public static ProjectProxyManager Instance { get; private set; }

        public ProjectProxyManager()
        {
            Instance = this;
        }

        internal ServerProjectInfo StartDownload(PublisherNetworkClient client, ProjectProxyStartDownloadRequestModel request)
        {
            ServerProjectInfo proj = null;

            if (client.IsPatchClient == false || client.PatchProjectMap.TryGetValue(request.ProjectId, out proj) == false)
                return null;

            proj.StartDownload(client, request.TransportMode);

            return proj;
        }

        internal void FinishDownload(PublisherNetworkClient client, ProjectProxyEndDownloadRequestModel request)
        {
            client.PatchDownloadProject?.EndDownload(client, true);
        }

        public async Task<PatchClientNetwork> ConnectProxyClient(ServerProjectInfo project)
        {
            var client = GetClient(project.Info.PatchInfo.IpAddress, project.Info.PatchInfo.Port, () => {

                var c = new PatchClientNetwork(project.Info.PatchInfo);

                return c;
            });

            client.ProjectMap.TryAdd(project.Info.Id, project);

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
            if (client.PatchProjectMap.TryGetValue(projectId, out var project))
                project.SignOutPatchClient(client);
        }
    }
}
