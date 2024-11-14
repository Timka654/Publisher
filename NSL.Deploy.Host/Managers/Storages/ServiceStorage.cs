using ServerPublisher.Server.Info;
using System.Collections.Concurrent;

namespace ServerPublisher.Server.Managers.Storages
{
    internal class ServiceStorage
    {

        protected ConcurrentDictionary<ServerProjectInfo, ProjectServiceInfo> serviceMap = new ConcurrentDictionary<ServerProjectInfo, ProjectServiceInfo>();

        protected bool AddService(ProjectServiceInfo service)
        {
            return serviceMap.TryAdd(service.ProjectInfo, service);
        }
        
        protected ProjectServiceInfo GetService(ServerProjectInfo project)
        {
            serviceMap.TryGetValue(project,out var service);

            return service;
        }

        protected bool RemoveService(ProjectServiceInfo service)
        {
            return serviceMap.TryAdd(service.ProjectInfo, service);
        }

        protected ServiceStorage()
        {

        }
    }
}
