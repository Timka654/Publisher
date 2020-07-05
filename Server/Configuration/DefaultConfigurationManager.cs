using ConfigurationEngine;
using ConfigurationEngine.Info;
using ConfigurationEngine.Providers.Json;
using Publisher.Server.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Configuration
{
    public class DefaultConfigurationManager : IConfigurationManager<DefaultConfigurationManager>
    {
        private static readonly List<ConfigurationInfo> DefaultValues = new List<ConfigurationInfo>()
        {
            //new ConfigurationInfo("project/directory","","")
        };

        public DefaultConfigurationManager(string fileName, char nodeSeparator = '/') : base(fileName, nodeSeparator)
        {
            Provider = new LoadingProvider();
            OnLog += StaticInstances.ServerLogger.Append;
            SetDefaults(DefaultValues,false);
        }
    }
}
