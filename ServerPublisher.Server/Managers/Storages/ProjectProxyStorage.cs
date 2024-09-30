using ServerPublisher.Server.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerPublisher.Server.Managers.Storages
{
    internal class ProjectProxyStorage
    {
        protected ConcurrentDictionary<(string, int), Lazy<PatchClientNetwork>> connection_map = new();

        protected void RemoveClient(PatchClientNetwork network)
        {
            RemoveClient(network.IpAddress, network.Port);
        }

        protected void RemoveClient(string ipAddress, int port)
        {
            connection_map.Remove((ipAddress, port), out var dummy);
        }

        protected PatchClientNetwork GetClient(string ipAddress, int port, Func<PatchClientNetwork> clientLoader)
        {
            return connection_map.GetOrAdd((ipAddress, port), (id) => new Lazy<PatchClientNetwork>(clientLoader)).Value;
        }

        protected ProjectProxyStorage()
        {

        }
    }
}
