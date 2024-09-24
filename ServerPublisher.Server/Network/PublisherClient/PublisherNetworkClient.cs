using ServerPublisher.Server.Info;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using ServerPublisher.Shared.Enums;
using System.Collections.Concurrent;

namespace ServerPublisher.Server.Network.PublisherClient
{
    public class PublisherNetworkClient : IServerNetworkClient, IProcessFileContainer, IDisposable
    {
        public UserInfo UserInfo { get; set; }

        public ServerProjectInfo ProjectInfo => UserInfo.CurrentProject;

        public ProjectFileInfo CurrentFile { get; set; }

        public bool Compressed { get; set; }

        public ProxyClientContextDataModel? ProxyClientContext { get; set; }

        public Dictionary<string, ServerProjectInfo> PatchProjectMap { get; set; } = null;

        public OSTypeEnum? Platform { get; internal set; }

        private List<EventWaitHandle> lockers = new List<EventWaitHandle>();

        private AutoResetEvent safeLockers = new AutoResetEvent(true);

        public void Lock(EventWaitHandle handle, int timeout = Timeout.Infinite)
        {
            safeLockers.WaitOne(1000);

            handle.WaitOne(timeout);

            if (lockers == null)
            {
                if (handle is AutoResetEvent)
                {
                    handle.Set();
                }

                return;
            }

            if (handle is not AutoResetEvent)
            {
                handle.Reset();
            }

            lockers.Add(handle);

            safeLockers.Set();
        }

        public void Unlock(EventWaitHandle handle)
        {
            safeLockers.WaitOne(1000);
            if (lockers != null)
            {
                lockers.Remove(handle);
            }

            handle.Set();

            safeLockers.Set();
        }

        public void Dispose()
        {
            safeLockers.WaitOne(1000);

            var l = lockers.ToArray();

            lockers = null;

            foreach (var item in l)
            {
                item.Set();
            }

            safeLockers.Set();

        }
    }

    public class ProxyClientContextDataModel
    {
        public ConcurrentDictionary<string, ServerProjectInfo> PatchProjectMap { get; } = new ();
    }
}
