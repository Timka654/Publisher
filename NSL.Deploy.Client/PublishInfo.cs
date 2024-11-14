namespace ServerPublisher.Client
{
    public class PublishInfo
    {
        public int BufferLen { get; set; } = 409088;

        public string PublishDirectory { get; set; }

        public string ProjectId { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; } = 6583;

        public string SuccessArgs { get; set; }

        public string AuthKeyPath { get; set; }

        public string InputKey { get; set; }

        public string OutputKey { get; set; }

        public bool HasCompression { get; set; } = false;
    }
}
