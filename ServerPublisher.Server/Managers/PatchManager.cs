using ServerPublisher.Server.Managers.Storages;
using ServerPublisher.Server.Network;
using ServerPublisher.Server.Info;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Shared;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(0)]
    internal class PatchManager : PatchStorage
    {
        public static PatchManager Instance { get; private set; }

        public PatchManager()
        {
            Instance = this;
        }


        internal void StartDownload(PublisherNetworkClient client, string projectId, TransportModeEnum transportMode)
        {
            ServerProjectInfo proj = null;

            if (client.IsPatchClient == false || client.PatchProjectMap.TryGetValue(projectId, out proj) == false)
                PatchServerPacketRepository.SendStartDownloadResult(client, false, new List<string>());
            proj.StartDownload(client, transportMode);
        }

        internal void FinishDownload(PublisherNetworkClient client)
        {
            client.PatchDownloadProject?.EndDownload(client, true);
        }

        public async Task<PatchClientNetwork> LoadProjectPatchClient(ServerProjectInfo project)
        {
            var client = GetClient(project.Info.PatchInfo.IpAddress, project.Info.PatchInfo.Port);

            if (client == null)
            {
                client = PatchClientNetwork.Load(project.Info.PatchInfo);
                await client.ConnectAsync();
            }

            client.ProjectMap.TryAdd(project.Info.Id, project);

            return client;
        }

        internal void SignIn(PublisherNetworkClient client, string projectId, string userId, byte[] key, DateTime latestUpdate)
        {
            var project = StaticInstances.ProjectsManager.GetProject(projectId);

            if (project == null)
            {
                PatchServerPacketRepository.SendSignInResult(client, SignStateEnum.ProjectNotFound);

                return;
            }

             project.SignPatchClient(client, userId, key, latestUpdate);
        }

        internal void SignOut(PublisherNetworkClient client, string projectId)
        {
            if (client.PatchProjectMap.TryGetValue(projectId, out var project))
                project.SignOutPatchClient(client);
        }
    }
}
