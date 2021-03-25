using Publisher.Server.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Info
{
    interface IProcessFileContainer
    {
        ProjectFileInfo CurrentFile { get; set; }
    }
}
