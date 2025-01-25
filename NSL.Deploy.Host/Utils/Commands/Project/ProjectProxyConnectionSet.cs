using ServerPublisher.Server.Info;
using NSL.Logger;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.Project
{
    [CLHandleSelect("projects")]
    [CLArgument("ip_address", typeof(string))]
    [CLArgument("port", typeof(int))]
    [CLArgument("identity_name", typeof(string))]
    [CLArgument("input_cipher_key", typeof(string), true)]
    [CLArgument("output_cipher_key", typeof(string), true)]
    [CLArgument("project_id", typeof(string), optional: true)]
    [CLArgument("directory", typeof(string), optional: true)]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class ProjectProxyConnectionSet : CLHandler
    {
        public override string Command => "set_patch_connection";

        public override string Description { get => ""; set => base.Description = value; }

        public ProjectProxyConnectionSet()
        {
            AddArguments(SelectArguments());
        }
        [CLArgumentValue("ip_address")] public string IpAddress { get; set; }

        [CLArgumentValue("port")] public int Port { get; set; }

        [CLArgumentValue("identity_name")] public string IdentityName { get; set; }

        [CLArgumentExists("input_cipher_key")] public bool ExistsInputCipherKey { get; set; }

        [CLArgumentValue("input_cipher_key")] public string InputCipherKey { get; set; }

        [CLArgumentExists("output_cipher_key")] public bool ExistsOutputCipherKey { get; set; }

        [CLArgumentValue("output_cipher_key")] public string OutputCipherKey { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Add Patch Connection");

            if (!ExistsInputCipherKey)
            {
                InputCipherKey = PublisherServer.Configuration.Publisher.Server.Cipher.OutputKey;

                AppCommands.Logger.AppendInfo($"Not contains \"input_cipher_key\" parameter. Set from configuration {InputCipherKey}");
            }

            if (!ExistsOutputCipherKey)
            {
                OutputCipherKey = PublisherServer.Configuration.Publisher.Server.Cipher.InputKey;

                AppCommands.Logger.AppendInfo($"Not contains \"output_cipher_key\" parameter. Set from configuration {OutputCipherKey}");
            }

            ServerProjectInfo projectInfo = values.GetProject();

            if (projectInfo != null)
            {
                if (!values.ConfirmCommandAction(AppCommands.Logger))
                    return CommandReadStateEnum.Failed;

                projectInfo.UpdatePatchInfo(new ProjectPatchInfo()
                {
                    IpAddress = IpAddress,
                    Port = Port,
                    InputCipherKey = InputCipherKey,
                    OutputCipherKey = OutputCipherKey,
                    SignName = IdentityName
                });

                AppCommands.Logger.AppendInfo($"Patch connection info changed in {projectInfo.Info.Name}({projectInfo.Info.Id}) project");
            }

            return CommandReadStateEnum.Success;
        }
    }
}
