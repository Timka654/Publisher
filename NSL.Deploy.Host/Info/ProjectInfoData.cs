using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSL.Generators.FillTypeGenerator.Attributes;
using ServerPublisher.Shared.Info;
using System;
using System.Collections.Generic;

namespace ServerPublisher.Server.Info
{
    [FillTypeGenerate(typeof(ProjectInfoData), "Updatable")]
    public partial class ProjectInfoData
    {
        [FillTypeGenerateInclude("Updatable")]
        public string Id { get; set; }

        [FillTypeGenerateInclude("Updatable")]
        public string Name { get; set; }

        [FillTypeGenerateInclude("Updatable")]
        public bool FullReplace { get; set; }

        [FillTypeGenerateInclude("Updatable")]
        public bool Backup { get; set; }

        [FillTypeGenerateInclude("Updatable")]
        public List<string> IgnoreFilePaths { get; set; } = [];

        public DateTime? LatestUpdate { get; set; }

        public ProjectPatchInfo PatchInfo { get; set; }

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        [ConfigurationKeyName("scripts.references"), JsonProperty("scripts.references")]
        public ConfigurationSettingsInfo__Project_Configuration__Server__ScriptReference[] ScriptsReferences { get; set; } = [];
    }
}
