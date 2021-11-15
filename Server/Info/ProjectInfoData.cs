using Publisher.Basic;
using System;
using System.Collections.Generic;

namespace Publisher.Server.Info
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
    }
}
