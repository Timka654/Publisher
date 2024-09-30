using Microsoft.Extensions.Configuration;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public class ConfigurationSettingsInfo
    {
        public ConfigurationSettingsInfo__Publisher Publisher { get; set; } = new();
    }

    public class ConfigurationSettingsInfo__Publisher
    {
        public ConfigurationSettingsInfo__Publisher__Server Server { get; set; } = new();

        public ConfigurationSettingsInfo__Publisher__Proxy Proxy { get; set; } = new();

        [ConfigurationKeyName("project_configuration")]
        public ConfigurationSettingsInfo__Project_Configuration ProjectConfiguration { get; set; } = new();

        public ConfigurationSettingsInfo__Publisher__Service Service { get; set; } = new();
    }

    public class ConfigurationSettingsInfo__Publisher__Service
    {
        [ConfigurationKeyName("use_integrate")]
        public bool UseIntegrate { get; set; }
    }

    public class ConfigurationSettingsInfo__Publisher__Server
    {
        public ConfigurationSettingsInfo__Publisher__Server__IO IO { get; set; } = new();

        public ConfigurationSettingsInfo__Publisher__Server__Cipher Cipher { get; set; } = new();
    }

    public class ConfigurationSettingsInfo__Publisher__Proxy
    {
        [ConfigurationKeyName("buffer_size")]
        public int BufferSize { get; set; } = 409600;
    }

    public class ConfigurationSettingsInfo__Publisher__Server__IO
    {
        public string Address { get; set; } = "*";

        public int Port { get; set; } = 6583;

        [ConfigurationKeyName("buffer_size")]
        public int BufferSize { get; set; } = 409600;

        public int Backlog { get; set; } = 100;
    }

    public class ConfigurationSettingsInfo__Publisher__Server__Cipher
    {
        [ConfigurationKeyName("input_key")]
        public string? InputKey { get; set; } = "!{b1HX11R**";

        [ConfigurationKeyName("output_key")]
        public string? OutputKey { get; set; } = "!{b1HX11R**";

    }

    public class ConfigurationSettingsInfo__Project_Configuration
    {
        public ConfigurationSettingsInfo__Project_Configuration__Server Server { get; set; } = new();

        public ConfigurationSettingsInfo__Project_Configuration__Values Base { get; set; } = new();

        public ConfigurationSettingsInfo__Project_Configuration__Values Default { get; set; } = new();
    }

    public class ConfigurationSettingsInfo__Project_Configuration__Server
    {
        [ConfigurationKeyName("library.file.path")]
        public string LibraryFilePath { get; set; } = Path.Combine("data", "projects.json");

        [ConfigurationKeyName("global.both.users.folder.path")]
        public string GlobalBothUsersFolderPath { get; set; } = Path.Combine("data", "global", "users", "both");

        [ConfigurationKeyName("global.publish.users.folder.path")]
        public string GlobalPublishUsersFolderPath { get; set; } = Path.Combine("data", "global", "users", "publish");

        [ConfigurationKeyName("global.proxy.users.folder.path")]
        public string GlobalProxyUsersFolderPath { get; set; } = Path.Combine("data", "global", "users", "proxy");

        [ConfigurationKeyName("global.scripts.folder.path")]
        public string GlobalScriptsFolderPath { get; set; } = Path.Combine("data", "global", "scripts");

        [ConfigurationKeyName("scripts.default.usings")]
        public string[] ScriptsDefaultUsings { get; set; } = [
            "System",
            "System.IO",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Diagnostics",
            "ServerPublisher.Server.Info",
            "ServerPublisher.Server.Scripts"
            ];
    }

    public class ConfigurationSettingsInfo__Project_Configuration__Values
    {
        [ConfigurationKeyName("ignore.file.paths")]
        public string[] IgnoreFilePaths { get; set; } = [];
    }
}
