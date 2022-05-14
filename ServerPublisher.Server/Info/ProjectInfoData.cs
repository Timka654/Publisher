using ServerPublisher.Shared;
using System;
using System.Collections.Generic;

namespace ServerPublisher.Server.Info
{
    public class ProjectInfoData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool FullReplace { get; set; }

        public bool Backup { get; set; }

        public bool PreventScriptExecution { get; set; } = false;

        public List<string> IgnoreFilePaths { get; set; }

        public DateTime? LatestUpdate { get; set; }

        public ProjectPatchInfo PatchInfo { get; set; }

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
