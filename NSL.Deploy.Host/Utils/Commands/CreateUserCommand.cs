using ServerPublisher.Server.Info;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("name", typeof(string))]
    [CLArgument("global", typeof(CLContainsType))]
    [CLArgument("publisher", typeof(CLContainsType))]
    [CLArgument("proxy", typeof(CLContainsType))]
    [CLArgument("both", typeof(CLContainsType))]
    internal class CreateUserCommand : CLHandler
    {
        public override string Command => "create_user";

        public override string Description { get => ""; set => base.Description = value; }

        public CreateUserCommand()
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
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create user");

            if (global)
            {
                AppCommands.Logger.AppendInfo("Create global user");

                if (publisher)
                {
                    AppCommands.Logger.AppendInfo("Create publisher user");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var user = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalPublishUserStorage.AddUser(user))
                        AppCommands.Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{user.Name} already exist");
                    }

                }
                else if (proxy)
                {
                    AppCommands.Logger.AppendInfo("Create proxy user");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var user = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalProxyUserStorage.AddUser(user))
                        AppCommands.Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{user.Name} already exist");
                    }
                }
                else if (both)
                {
                    AppCommands.Logger.AppendInfo("Create publisher/proxy user");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    var user = UserInfo.CreateUser(name);

                    if (PublisherServer.ProjectsManager.GlobalBothUserStorage.AddUser(user))
                        AppCommands.Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        AppCommands.Logger.AppendError($"{user.Name} already exist");
                    }
                }


                return CommandReadStateEnum.Success;
            }

            ServerProjectInfo projectInfo = values.GetProject();

            if (projectInfo != null)
            {
                if (!values.ConfirmCommandAction(AppCommands.Logger))
                    return CommandReadStateEnum.Cancelled;

                var user = UserInfo.CreateUser(name);

                if (projectInfo.AddUser(user))
                    AppCommands.Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                else
                {
                    AppCommands.Logger.AppendError($"{user.Name} already exist in project {projectInfo.Info.Name}({projectInfo.Info.Id})");
                }
            }

            return CommandReadStateEnum.Success;
        }
    }
}
