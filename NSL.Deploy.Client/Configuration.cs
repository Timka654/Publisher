using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPublisher.Client
{
    public class ConfigurationInfoModel
    {
        public string TemplatesPath { get; set; } = "%APPLICATIONAPPDATA%/templates";

        public string KeysPath { get; set; } = "%APPLICATIONAPPDATA%/key_storage";
    }
}
