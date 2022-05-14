//Signature for tests only

#if !Release

using System;

namespace ServerPublisher.Server.Scripts
{
    internal class ScriptableServerProjectInfo : IScriptableServerProjectInfo
    {
        public string ProjectDirPath { get; set; }

        public void BroadcastMessage(string log)
        {
            Console.WriteLine(log);
        }
    }
}

#endif