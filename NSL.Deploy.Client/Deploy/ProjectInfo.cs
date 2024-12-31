using ServerPublisher.Shared.Info;
using System.Collections.Generic;

namespace NSL.Deploy.Client.Deploy
{
    public class ProjectInfo
    {
        public string ProjectId { get; set; }

        public string PublishDirectory { get; set; }

        public string Ip { get; set; }

        public string InputKey { get; set; }

        public string OutputKey { get; set; }

        public BasicUserInfo Identity { get; set; }


        public int Port { get; set; } = 6583;

        public int BufferLen { get; set; } = 64 * 1024 - 64;

        public Dictionary<string, string> SuccessArgs { get; set; } = new Dictionary<string, string>();

        public bool HasCompression { get; set; } = false;
    }
}
