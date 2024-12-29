﻿using NSL.Utils.CommandLine.CLHandles;

namespace NSL.Deploy.Host.Utils.Commands.User
{
    [CLHandleSelect("default")]
    internal class IdentityCommand : CLHandler
    {
        public override string Command => "identity";

        public override string Description { get => ""; set => base.Description = value; }

        public IdentityCommand()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("identity", true));
        }
    }
}