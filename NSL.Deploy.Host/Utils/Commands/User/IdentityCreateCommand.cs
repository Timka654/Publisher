using ServerPublisher.Server.Info;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.User
{
    [CLHandleSelect("identity")]
    [CLArgument("name", typeof(string))]
    [CLArgument("global", typeof(CLContainsType))]
    [CLArgument("publisher", typeof(CLContainsType))]
    [CLArgument("proxy", typeof(CLContainsType))]
    [CLArgument("both", typeof(CLContainsType))]
    [CLArgument("projectId", typeof(string), optional: true)]
    [CLArgument("directory", typeof(string), optional: true)]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class IdentityCreateCommand : CLHandler
    {
        public override string Command => "create";

        public override string Description { get => ""; set => base.Description = value; }

        public IdentityCreateCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("name")] string name { get; set; }

        [CLArgumentExists("global")] bool global { get; set; }
        [CLArgumentExists("proxy")] bool proxy { get; set; }
        [CLArgumentExists("both")] bool both { get; set; }
        [CLArgumentExists("publisher")] bool publisher { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create identity");

            if (global)
            {
                AppCommands.Logger.AppendInfo("Create global identity");

                if (publisher)
                {
                    AppCommands.Logger.AppendInfo("Create publisher identity");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var identity = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalPublishUserStorage.AddUser(identity))
                        AppCommands.Logger.AppendInfo($"Identity {identity.Name} by id {identity.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{identity.Name} already exist");
                    }

                }
                else if (proxy)
                {
                    AppCommands.Logger.AppendInfo("Create proxy identity");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var identity = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalProxyUserStorage.AddUser(identity))
                        AppCommands.Logger.AppendInfo($"identity {identity.Name} by id {identity.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{identity.Name} already exist");
                    }
                }
                else if (both)
                {
                    AppCommands.Logger.AppendInfo("Create publisher/proxy identity");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var identity = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalBothUserStorage.AddUser(identity))
                        AppCommands.Logger.AppendInfo($"identity {identity.Name} by id {identity.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{identity.Name} already exist");
                    }
                }


                return CommandReadStateEnum.Success;
            }

            ServerProjectInfo projectInfo = values.GetProject();

            if (projectInfo != null)
            {
                if (!values.ConfirmCommandAction(AppCommands.Logger))
                    return CommandReadStateEnum.Cancelled;

                var identity = UserInfo.CreateUser(name);

                if (projectInfo.AddUser(identity))
                    AppCommands.Logger.AppendInfo($"identity {identity.Name} by id {identity.Id} success created");
                else
                {
                    AppCommands.Logger.AppendError($"{identity.Name} already exist in project {projectInfo.Info.Name}({projectInfo.Info.Id})");
                }
            }

            return CommandReadStateEnum.Success;
        }
    }
}
