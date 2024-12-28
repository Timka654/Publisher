using ServerPublisher.Server.Info;
using System;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using Newtonsoft.Json;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server.Utils;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class ConfigurationDefaultCommand : CLHandler
    {
        public override string Command => "cdefault";

        public override string Description { get => ""; set => base.Description = value; }

        public ConfigurationDefaultCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            AppCommands.Logger.AppendInfo("Configuration reset to default");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json").GetNormalizedPath(), JsonConvert.SerializeObject(new ConfigurationSettingsInfo(), JsonUtils.JsonSettings));

            return CommandReadStateEnum.Success;
        }
    }
}
