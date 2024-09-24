using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPublisher.Shared.Models
{
    public class FileDownloadDataModel
    {
        public string RelativePath { get; set; }

        public byte[] Data { get; set; }
    }
}
