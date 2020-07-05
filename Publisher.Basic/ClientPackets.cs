using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Basic
{
    public enum ClientPackets
    {
        SignInResult = 1,
        FileListResult,

        ServerLog,
        RunNotify,
        ProjectPublishStart,
        ProjectPublishEndResult,
        ProjectFileStartResult,
        UploadFileResult
    }
}
