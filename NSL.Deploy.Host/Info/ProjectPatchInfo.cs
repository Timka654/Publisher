using NSL.Generators.FillTypeGenerator.Attributes;

namespace ServerPublisher.Shared.Info
{
    [FillTypeGenerate(typeof(ProjectPatchInfo))]
    public partial class ProjectPatchInfo
    {
        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string SignName { get; set; }

        public string InputCipherKey { get; set; }

        public string OutputCipherKey { get; set; }
    }
}
