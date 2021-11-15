using Publisher.Server.Info;
using SCL;
using SocketCore.Utils;
using System.Collections.Generic;
using System.Threading;

namespace Publisher.Server.Network
{
    public class NetworkPatchClient : BaseSocketNetworkClient, IProcessFileContainer
    {
        public ProjectFileInfo CurrentFile { get; set; }

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
}
