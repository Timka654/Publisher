using ServerPublisher.Server.Network;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerPublisher.Server.Managers.Storages
{
    internal class PatchStorage
    {
        protected ConcurrentDictionary<(string, int), PatchClientNetwork> connection_map = new ConcurrentDictionary<(string, int), PatchClientNetwork>();

        protected void AddClient(PatchClientNetwork network)
        {
            if (connection_map.TryAdd((network.Options.IpAddress, network.Options.Port), network) == false)
            {
                connection_map[(network.Options.IpAddress, network.Options.Port)] = network;
            }
        }

        protected void RemoveClient(PatchClientNetwork network)
        {
            RemoveClient(network.Options.IpAddress, network.Options.Port);
        }

        protected void RemoveClient(string ipAddress, int port)
        {
            connection_map.Remove((ipAddress, port), out var dummy);
        }

        protected PatchClientNetwork GetClient(string ipAddress, int port)
        {
            connection_map.TryGetValue((ipAddress, port), out var network);
            return network;
        }

        protected PatchStorage()
        {

        }
    }
}
