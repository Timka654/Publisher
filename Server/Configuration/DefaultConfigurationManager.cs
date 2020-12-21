using ConfigurationEngine;
using ConfigurationEngine.Info;
using ConfigurationEngine.Providers.Json;
using System.Collections.Generic;

namespace Publisher.Server.Configuration
{
    public class DefaultConfigurationManager : IConfigurationManager<DefaultConfigurationManager>
    {
        private static readonly List<ConfigurationInfo> DefaultValues = new List<ConfigurationInfo>()
        {
            //new ConfigurationInfo("project/directory","","")
        };

        public DefaultConfigurationManager(string fileName) : base(fileName)
        {
            Provider = new LoadingProvider();
            OnLog += StaticInstances.ServerLogger.Append;
            SetDefaults(DefaultValues,false);
        }
    }
}
