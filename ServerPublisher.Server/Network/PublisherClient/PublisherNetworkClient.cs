using ServerPublisher.Server.Info;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using ServerPublisher.Shared.Enums;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace ServerPublisher.Server.Network.PublisherClient
{
    public class PublisherNetworkClient : IServerNetworkClient, IDisposable
    {
        public UserInfo UserInfo { get; set; }

        public ProjectPublishContext? PublishContext { get; set; }

        public ProxyClientContextDataModel? ProxyClientContext { get; set; }

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

    public class ProxyClientContextDataModel : IDisposable
    {
        public required PublisherNetworkClient Network { get; set; }

        public ConcurrentDictionary<string, ServerProjectInfo> PatchProjectMap { get; } = new();

        public ConcurrentDictionary<string, ProxyClientDownloadContext> ProcessingProjects { get; } = new();

        public void Dispose()
        {
            foreach (var item in ProcessingProjects.Values.ToArray())
            {
                ProcessingProjects.TryRemove(item.ProjectInfo.Info.Id, out _);
                item.Dispose();
            }
        }

        internal Guid AddProcessingFile(ServerProjectInfo serverProjectInfo, ProjectFileInfo file)
        {
            Guid fileId = default;

            if (ProcessingProjects.TryGetValue(serverProjectInfo.Info.Id, out var downloadContext))
                while (!downloadContext.TempFileMap.TryAdd(fileId = Guid.NewGuid(), file.OpenRead())) ;

            return fileId;
        }

        internal Stream GetProcessingFile(string projectId, Guid fileId)
        {
            if (ProcessingProjects.TryGetValue(projectId, out var downloadContext))
                if (downloadContext.TempFileMap.TryGetValue(fileId, out var file))
                    return file;

            return default;
        }

        internal void DisposeProcessingFile(string projectId, Guid fileId)
        {
            if (ProcessingProjects.TryGetValue(projectId, out var downloadContext))
                if (downloadContext.TempFileMap.TryRemove(fileId, out var file))
                    file.Dispose();
        }
    }

    public class ProxyClientDownloadContext : IDisposable
    {
        public required ProxyClientContextDataModel Context { get; set; }

        public required ServerProjectInfo ProjectInfo { get; set; }

        public ConcurrentDictionary<Guid, Stream> TempFileMap { get; } = new();

        bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            foreach (var item in TempFileMap)
            {
                item.Value.Dispose();
            }

            TempFileMap.Clear();

            ProjectInfo?.EndDownload(Context.Network, false);
        }
    }
}
