using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Basic
{
    public enum ServerPackets
    {
        SignIn = 1,
        ProjectFileList,
        
        ProjectStart,
        ProjectEnd,

        ProjectFileStart,
        ProjectFileStop,
        UploadFile
    }
}
