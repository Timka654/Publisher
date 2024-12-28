using ServerPublisher.Server.Info;
using System;
using System.IO;
using NSL.Logger;
using System.Diagnostics;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;

namespace NSL.Deploy.Host.Utils.Commands
{
    internal static class CommandsHelper
    {
        public static bool GetWorkingDirectory(this CLArgumentValues args, string name, out string value)
        {
            if (args.TryGetValue<string>(name, out var _value))
            {
                value = _value;

                return true;
            }

            value = Directory.GetCurrentDirectory();

            AppCommands.Logger.AppendInfo($"Cannot find parameter \"{name}\". Set current directory - \"{value}\"");

            return false;
        }

        public static bool TryGetCommandValue<T>(this CommandLineArgs args, string key, out T result)
        {
            if (args.TryGetOutValue(key, out result))
            {
                AppCommands.Logger.AppendInfo($"\"{key}\" = \"{result}\"");

                return true;
            }

            AppCommands.Logger.AppendInfo($"\"{key}\" = <none>");

            return false;
        }

        public static ServerProjectInfo? GetProject(this CLArgumentValues args)
        {
            ServerProjectInfo? projectInfo;

            if (args.TryGetValue("project_id", out string projectId))
            {
                if (!Guid.TryParse(projectId, out var _))
                {
                    AppCommands.Logger.AppendError($"Invalid \"project_id\" parameter format - must have GUID format");
                    PidOrDirInfo();
                    return null;
                }

                projectInfo = PublisherServer.ProjectsManager.GetProject(projectId);

                if (projectInfo == null)
                {
                    AppCommands.Logger.AppendError($"Cannot find project by project_id = \"{projectId}\"");
                    PidOrDirInfo();
                }
            }
            else
            {
                AppCommands.Logger.AppendError($"Cannot find project_id parameter. Try get by directory");

                args.GetWorkingDirectory("directory", out var directory);

                projectInfo = PublisherServer.ProjectsManager.GetProjectByPath(directory);

                if (projectInfo == null)
                {
                    AppCommands.Logger.AppendError($"Cannot find project in \"{directory}\"");
                    PidOrDirInfo();
                }
            }

            return projectInfo;
        }

        public static void PidOrDirInfo()
        {
            AppCommands.Logger.AppendError($"Current command must have project_id(has GUID format) or directory parameters for identity project");
            AppCommands.Logger.AppendError($"You can not using identity parameters if executing command from directory contains project");
        }

        public static void TerminalExecute(this CLHandler c, string command)
        {
            Process.Start("/bin/bash", $"-c \"{command}\"");
        }
    }
}
